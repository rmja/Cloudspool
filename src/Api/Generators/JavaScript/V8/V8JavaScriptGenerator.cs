using Api.Generators.JavaScript.Polyfills;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Generators.JavaScript.V8
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

        public async Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null, CancellationToken cancellationToken = default)
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
    const { contentType } = await import('main');
    return contentType;
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
    }
}
