using ChakraCore.API;
using System;
using System.IO;

namespace Api.Generators.TypeScript
{
    public class ChakraCoreTypeScriptTranspiler : ITypeScriptTranspiler, IDisposable
    {
        private readonly JavaScriptRuntime _runtime;
        private readonly JavaScriptContext _context;

        public ChakraCoreTypeScriptTranspiler()
        {
            using var stream = typeof(ChakraCoreTypeScriptTranspiler).Assembly.GetManifestResourceStream(typeof(V8TypeScriptTranspiler), "typescriptServices.js");
            using var reader = new StreamReader(stream);
            var typescriptServicesSource = reader.ReadToEnd();

            _runtime = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.None);
            _context = JavaScriptContext.CreateContext(_runtime);

            // Install the compiler in the context
            using (var scope = _context.GetScope())
            {
                JavaScriptContext.RunScript(typescriptServicesSource);
            }
        }

        public string Transpile(string input)
        {
            lock (this)
            {
                using (var scope = _context.GetScope())
                { 
                    var transpileFunction = JavaScriptContext.RunScript(@"
input => {
    const result = ts.transpileModule(input, {
        compilerOptions: {
            target: ts.ScriptTarget.ES2015,
            module: ts.ModuleKind.ES2015,
            lib: [ 'ES2015' ]
        }
    });
    return result.outputText;
}");

                    var outputText = transpileFunction.CallFunction(JavaScriptValue.GlobalObject, JavaScriptValue.FromString(input));

                    return outputText.ToString();
                }
            }
        }

        public void Dispose()
        {
            _runtime.Dispose();
        }
    }
}
