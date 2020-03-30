using Microsoft.Extensions.Caching.Memory;
using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using JS = NiL.JS.BaseLibrary;

namespace Api.Generators.ECMAScript6
{
    public class ECMAScript6Generator : IGenerator
    {
        private readonly IMemoryCache _cache;

        public ECMAScript6Generator(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string[] ValidateTemplate(string code)
        {
            try
            {
                var script = Script.Parse(code);
                CreateBuilder(script, null);
                return Array.Empty<string>();
            }
            catch (JSException e)
            {
                return new[] { e.Message };
            }
        }

        public Task<(byte[] Content, string ContentType)> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null)
        {
            var script = _cache.GetOrCreate(code, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                return Script.Parse(code);
            });

            return GenerateDocumentAsync(script, model, resourceManager);
        }

        public async Task<(byte[] Content, string ContentType)> GenerateDocumentAsync(Script script, object model, IResourceManager resourceManager)
        {
            var (builder, build, contentType) = CreateBuilder(script, resourceManager);

            var json = JsonSerializer.Serialize(model, model?.GetType());
            var jsModel = JS.JSON.parse(json);
            var result = build.Call(builder, new Arguments() { jsModel });

            if (result.Value is JS.Promise promise)
            {
                result = await promise.Task;
            }

            var content = result.Value switch
            {
                string @string => Encoding.UTF8.GetBytes(@string),
                IEnumerable<KeyValuePair<string, JSValue>> enumerable => enumerable.Select(x => (byte)x.Value).ToArray(),
                _ => throw new GeneratorException("build did not produce a result"),
            };

            return (content, contentType);
        }

        static (JSObject Builder, ICallable Build, string ContentType) CreateBuilder(Script script, IResourceManager resourceManager)
        {
            var module = new Module("builder.js", script);
            module.Context.GlobalContext.CurrentTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            
            if (resourceManager is object)
            {
                var resourcesModuleResolver = new ResourcesModuleResolver(resourceManager);
                module.ModuleResolversChain.Add(resourcesModuleResolver);
                ExtendGlobalContext(module.Context.GlobalContext, resourcesModuleResolver);
            }
            else
            {
                ExtendGlobalContext(module.Context.GlobalContext, null);
            }

            module.Run();

            var Builder = module.Exports.Default.Value as JS.Function;

            if (Builder is null)
            {
                throw new GeneratorException("Template does not have a default class export");
            }

            var arguments = new Arguments();
            var builder = Builder.Construct(arguments) as JSObject;

            var contentType = builder.GetProperty("contentType").Value as string;
            var build = builder.GetProperty("build").Value as JS.Function;

            if (contentType is null || build is null)
            {
                throw new GeneratorException("The exported Builder class does not have a contentType property and a build method");
            }

            var buildMethod = build.As<ICallable>();

            return (builder, buildMethod, contentType);
        }

        static void ExtendGlobalContext(GlobalContext context, ResourcesModuleResolver resourcesModuleResolver)
        {
            var module = LoadModule("image-data.js");
            module.Run();
            context.Add("ImageData", module.Exports.Default);

            Func<string, JSValue> require = resourcePath =>
            {
                if (!resourcesModuleResolver.TryGetModule("/" + resourcePath, out var module)) {
                    return JSValue.Undefined;
                }
                module.Run();
                return module.Exports.Default;
            };

            context.Add("require", require);
        }

        static Module LoadModule(string fileName)
        {
            using var stream = typeof(ECMAScript6Generator).Assembly.GetManifestResourceStream(typeof(ECMAScript6Generator), "Modules." + fileName);
            using var reader = new StreamReader(stream);
            var code = reader.ReadToEnd();
            return new Module(fileName, code);
        }

        class ResourcesModuleResolver : CachedModuleResolverBase
        {
            private readonly IResourceManager _resources;
            private static readonly Regex _regex = new Regex(@"^\/resources/([a-zA-Z0-9_-]+(\.[a-z]+))$", RegexOptions.Compiled);
            private readonly Dictionary<string, Func<string, byte[], Module>> _moduleFactories = new Dictionary<string, Func<string, byte[], Module>>()
            {
                [".json"] = CreateJson,
                [".bin"] = CreateBin,
                [".bmp"] = CreateBmp
            };

            public ResourcesModuleResolver(IResourceManager resources)
            {
                _resources = resources;
            }

            public override bool TryGetModule(ModuleRequest moduleRequest, out Module result)
            {
                return TryGetModule(moduleRequest.AbsolutePath, out result);
            }

            public bool TryGetModule(string moduleAbsolutePath, out Module result)
            {
                var match = _regex.Match(moduleAbsolutePath);

                if (!match.Success)
                {
                    result = default;
                    return false;
                }

                var alias = match.Groups[1].Value;
                var extension = match.Groups[2].Value;

                if (!_moduleFactories.TryGetValue(extension, out var factory))
                {
                    result = default;
                    return false;
                }

                var resource = _resources.GetResource(alias);

                if (resource is null)
                {
                    result = default;
                    return false;
                }

                result = factory(moduleAbsolutePath, resource);
                return true;
            }

            static Module CreateJson(string path, byte[] resource)
            {
                var json = Encoding.UTF8.GetString(resource);
                var script = $"const resource = {json}; export default resource";
                return new Module(path, script);
            }

            static Module CreateBin(string path, byte[] resource)
            {
                var scriptBuilder = new StringBuilder(100 + 4 * resource.Length); // ',' and three digits per element
                scriptBuilder.Append("const resource = new Uint8Array([");
                scriptBuilder.AppendJoin(',', resource);
                scriptBuilder.Append("]);export default resource;");
                var script = scriptBuilder.ToString();
                return new Module(path, script);
            }

            static Module CreateBmp(string path, byte[] resource)
            {
                using var stream = new MemoryStream(resource);
                using var bitmap = new Bitmap(stream);

                var size = bitmap.Width * bitmap.Height * 4;
                var RGBA = ArrayPool<byte>.Shared.Rent(size);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    Marshal.Copy(bitmapData.Scan0, RGBA, 0, size);

                    var scriptBuilder = new StringBuilder(150 + 4 * size); // ',' and three digits per element
                    scriptBuilder.Append("const data = new Uint8ClampedArray([");
                    for (var offset = 0; offset < size; offset += 4)
                    {
                        var rgba = BitConverter.ToUInt32(RGBA, offset);

                        scriptBuilder.Append((rgba >> 16) & 0xFF); // R
                        scriptBuilder.Append(',');
                        scriptBuilder.Append((rgba >> 08) & 0xFF); // G
                        scriptBuilder.Append(',');
                        scriptBuilder.Append((rgba >> 00) & 0xFF); // B
                        scriptBuilder.Append(',');
                        scriptBuilder.Append((rgba >> 24) & 0xFF); // A
                        scriptBuilder.Append(',');
                    }
                    scriptBuilder.AppendFormat("]);const resource = new ImageData(data, {0});export default resource;", bitmap.Width);
                    var script = scriptBuilder.ToString();
                    return new Module(path, script);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                    ArrayPool<byte>.Shared.Return(RGBA);
                }
            }
        }
    }
}
