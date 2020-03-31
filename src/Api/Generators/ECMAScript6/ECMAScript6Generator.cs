using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Generators.ECMAScript6
{
    public class ECMAScript6Generator : IGenerator
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger _consoleLogger;

        public ECMAScript6Generator(IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            _cache = cache;
            _consoleLogger = loggerFactory.CreateLogger("ScriptConsole");
        }

        public string[] ValidateTemplate(string code)
        {
            using var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports);

            try
            {
                engine.Compile(new DocumentInfo("main") { Category = ModuleCategory.Standard }, code);

                return Array.Empty<string>();
            }
            catch (ScriptEngineException e)
            {
                return new[] { e.ErrorDetails };
            }
        }

        public async Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null)
        {
            using var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports);
            engine.DocumentSettings.Loader = new CustomLoader(engine.DocumentSettings.Loader, resourceManager);
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            engine.DocumentSettings.SearchPath = string.Join(';',
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Modules"));

            engine.Execute(new DocumentInfo() { Category = ModuleCategory.Standard }, "import ImageData from 'ImageData'; globalThis.ImageData = ImageData");

            engine.AddHostObject("console", new Console(_consoleLogger));

            engine.DocumentSettings.AddSystemDocument("main", ModuleCategory.Standard, code);

            dynamic setModel = engine.Evaluate(@"
let model;
const setModel = m => model = JSON.parse(m);
setModel");

            setModel(JsonSerializer.Serialize(model));

            dynamic contentTypePromise = engine.Evaluate(new DocumentInfo() { Category = ModuleCategory.Standard }, @"
async function getContentType() {
    const {ContentType} = await import('main');
    return ContentType;
}
getContentType()");
            var contentType = await ToTask(contentTypePromise);

            if (contentType is Undefined)
            {
                contentType = null;
            }

            dynamic resultPromise = engine.Evaluate(new DocumentInfo() { Category = ModuleCategory.Standard }, @"
import Builder from 'main';
let builder = new Builder();
Promise.resolve(builder.build(model));");

            var result = await ToTask(resultPromise);

            switch (result)
            {
                case string @string: return new GenerateResult() { Content = Encoding.UTF8.GetBytes(@string), ContentType = contentType ?? "text/plain" };
                case ITypedArray<byte> typedArray: return new GenerateResult() { Content = typedArray.ToArray(), ContentType = contentType ?? "application/octet-stream" };
                case IList list:
                    { 
                        var array = new byte[list.Count];
                        for (var i = 0; i < list.Count; i++)
                        {
                            array[i] = Convert.ToByte(list[i]);
                        }
                        return new GenerateResult() { Content = array, ContentType = contentType ?? "application/octet-stream" };
                    }
                default: throw new GeneratorException("build did not produce a result");
            }
        }

        private static Task<dynamic> ToTask(dynamic promise)
        {
            var tcs = new TaskCompletionSource<dynamic>();
            Action<object> onResolved = value => tcs.SetResult(value);
            Action<dynamic> onRejected = reason => tcs.SetException(new Exception(reason.message));
            promise.then(onResolved, onRejected);
            return tcs.Task;
        }

        public class Console
        {
            private readonly ILogger _logger;

            public Console(ILogger logger)
            {
                _logger = logger;
            }

            public void log(string message)
            {
                _logger.LogInformation(message);
            }
        }

        class CustomLoader : DocumentLoader
        {
            private readonly DocumentLoader _defaultLoader;
            private readonly IResourceManager _resourceManager;
            private static readonly Regex _regex = new Regex(@"^resources/([a-zA-Z0-9_-]+\.[a-z]+)$", RegexOptions.Compiled);
            private static readonly Dictionary<string, Func<string, byte[], Document>> _documentFactories = new Dictionary<string, Func<string, byte[], Document>>()
            {
                [".json"] = CreateJson,
                [".bin"] = CreateBin,
                [".bmp"] = CreateBmp
            };

            public CustomLoader(DocumentLoader defaultLoader, IResourceManager resourceManager)
            {
                _defaultLoader = defaultLoader;
                _resourceManager = resourceManager;
            }

            public override Document LoadDocument(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                var alias = GetResourceAlias(specifier);

                if (alias is object && _documentFactories.TryGetValue(Path.GetExtension(alias), out var factory))
                {
                    var resource = _resourceManager.GetResource(alias);

                    if (resource is object)
                    {
                        return factory(specifier, resource);
                    }
                }

                return _defaultLoader.LoadDocument(settings, sourceInfo, specifier, category, contextCallback);
            }

            public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                var alias = GetResourceAlias(specifier);

                if (alias is object && _documentFactories.TryGetValue(Path.GetExtension(alias), out var factory))
                {
                    var resource = await _resourceManager.GetResourceAsync(alias, default);

                    if (resource is object)
                    {
                        return factory(specifier, resource);
                    }
                }

                return await _defaultLoader.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback);
            }

            private static string GetResourceAlias(string modulePath)
            {
                var match = _regex.Match(modulePath);

                return match.Success ? match.Groups[1].Value : null;
            }

            static Document CreateJson(string path, byte[] resource)
            {
                var json = Encoding.UTF8.GetString(resource);
                var script = $"const resource = {json}; export default resource";
                return new StringDocument(new DocumentInfo(path) { Category = ModuleCategory.Standard }, script);
            }

            static Document CreateBin(string path, byte[] resource)
            {
                var scriptBuilder = new StringBuilder(100 + 4 * resource.Length); // ',' and three digits per element
                scriptBuilder.Append("const resource = new Uint8Array([");
                scriptBuilder.AppendJoin(',', resource);
                scriptBuilder.Append("]);export default resource;");
                var script = scriptBuilder.ToString();
                return new StringDocument(new DocumentInfo(path) { Category = ModuleCategory.Standard }, script);
            }

            static Document CreateBmp(string path, byte[] resource)
            {
                using var stream = new MemoryStream(resource);
                using var bitmap = new Bitmap(stream);

                var size = bitmap.Width * bitmap.Height * 4;
                var RGBA = ArrayPool<byte>.Shared.Rent(size);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    Marshal.Copy(bitmapData.Scan0, RGBA, 0, size);

                    var scriptBuilder = new StringBuilder(150 + 4 * size); // ',' and at most three digits per element
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
                    return new StringDocument(new DocumentInfo(path) { Category = ModuleCategory.Standard }, script);
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
