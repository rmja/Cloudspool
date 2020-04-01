using Microsoft.ClearScript.V8;
using System;
using System.IO;

namespace Api.Generators.TypeScript
{
    public class V8TypeScriptTranspiler : IDisposable
    {
        private readonly V8ScriptEngine _engine;
        private readonly dynamic _transpileModule;
        private readonly object _transpileOptions = new
        {
            compilerOptions = new
            {
                target = 2, // ts.ScriptTarget.ES2015
                module = 5, // ts.ModuleKind.ES2015
                lib = new[] { "ES2015" }
            }
        };

        public V8TypeScriptTranspiler()
        {
            // See https://github.com/microsoft/TypeScript/wiki/Using-the-Compiler-API#a-simple-transform-function

            using var stream = typeof(V8TypeScriptTranspiler).Assembly.GetManifestResourceStream(typeof(V8TypeScriptTranspiler), "typescriptServices.js");
            using var reader = new StreamReader(stream);
            var typescriptServicesSource = reader.ReadToEnd();

            _engine = new V8ScriptEngine();
            _engine.Execute(typescriptServicesSource);

            _transpileModule = _engine.Script.ts.transpileModule;
        }

        public string Transpile(string input)
        {
            var result = _transpileModule(input, _transpileOptions);

            return result.outputText;
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
