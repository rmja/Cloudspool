using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System;
using System.IO;
using System.Text.Json;
using JS = NiL.JS.BaseLibrary;

namespace Api.Generators.TypeScript
{
    public class TypeScriptTranspiler
    {
        private static readonly Func<string, JSValue, JSValue> _transpileModule;
        private static readonly JSValue _transpileOptions;

        static TypeScriptTranspiler()
        {
            // See https://github.com/microsoft/TypeScript/wiki/Using-the-Compiler-API#a-simple-transform-function

            using var stream = typeof(TypeScriptTranspiler).Assembly.GetManifestResourceStream(typeof(TypeScriptTranspiler), "typescriptServices.js");
            using var reader = new StreamReader(stream);
            var typescriptCompilerSource = reader.ReadToEnd();

            var module = new Module(typescriptCompilerSource);
            module.Context.Add("globalThis", JSObject.CreateObject());
            module.Run();

            var ts = module.Context.GetVariable("ts");

            var transpileModuleFunction = ts["transpileModule"].Value as JS.Function;
            _transpileModule = transpileModuleFunction.MakeDelegate<Func<string, object, JSValue>>();

            _transpileOptions = JS.JSON.parse(JsonSerializer.Serialize(new
            {
                compilerOptions = new
                {
                    target = 2, // ts.ScriptTarget.ES2015
                    module = 5, // ts.ModuleKind.ES2015
                    lib = new[] { "ES2015" }
                }
            }));
        }

        public (string OutputText, string[] Diagnostics) Transpile(string input)
        {
            var result = _transpileModule(input, _transpileOptions);

            return ((string)result["outputText"].Value, Array.Empty<string>());
        }
    }
}
