using Api.Generators.JavaScript.Polyfills;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Generators.JavaScript
{
    public class V8JavaScriptGenerator : IJavaScriptGenerator
    {
        private readonly ResourceScriptFactory _resourceScriptFactory;

        public V8JavaScriptGenerator(ResourceScriptFactory resourceScriptFactory)
        {
            _resourceScriptFactory = resourceScriptFactory;
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
            engine.Execute(PolyfillScripts.Get("ImageData"));

            engine.DocumentSettings.Loader = new CustomLoader(engine.DocumentSettings.Loader, _resourceScriptFactory, resourceManager);
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
                default: throw new NotSupportedException("Build did not produce a supported result");
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

        class CustomLoader : DocumentLoader
        {
            private readonly DocumentLoader _defaultLoader;
            private readonly IResourceManager _resourceManager;
            private readonly ResourceScriptFactory _resourceScriptFactory;

            public CustomLoader(DocumentLoader defaultLoader, ResourceScriptFactory resourceScriptFactory, IResourceManager resourceManager)
            {
                _defaultLoader = defaultLoader;
                _resourceManager = resourceManager;
                _resourceScriptFactory = resourceScriptFactory;
            }

            public override Document LoadDocument(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                var alias = ResourceModuleUtils.GetResourceAlias(specifier);

                if (alias is object)
                {
                    var resource = _resourceManager.GetResource(alias);

                    if (resource is object)
                    {
                        var script = _resourceScriptFactory.CreateFromExtension(resource, Path.GetExtension(alias));

                        if (script is object)
                        {
                            return new StringDocument(new DocumentInfo(specifier) { Category = ModuleCategory.Standard }, script);
                        }
                    }
                }

                return _defaultLoader.LoadDocument(settings, sourceInfo, specifier, category, contextCallback);
            }

            public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                var alias = ResourceModuleUtils.GetResourceAlias(specifier);

                if (alias is object)
                {
                    var resource = await _resourceManager.GetResourceAsync(alias);

                    if (resource is object)
                    {
                        var script = _resourceScriptFactory.CreateFromExtension(resource, Path.GetExtension(alias));

                        if (script is object)
                        {
                            return new StringDocument(new DocumentInfo(specifier) { Category = ModuleCategory.Standard }, script);
                        }
                    }
                }

                return await _defaultLoader.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback);
            }
        }
    }
}
