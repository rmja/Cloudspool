using ChakraHost.Hosting;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Api.Generators.TypeScript
{
    public class ChakraCoreTypeScriptTranspiler : ITypeScriptTranspiler, IDisposable
    {
        private readonly JavaScriptRuntime _runtime;
        private readonly JavaScriptContext _context;
        private JavaScriptSourceContext _currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);

        public ChakraCoreTypeScriptTranspiler()
        {
            using var stream = typeof(ChakraCoreTypeScriptTranspiler).Assembly.GetManifestResourceStream(typeof(V8TypeScriptTranspiler), "typescriptServices.js");
            using var reader = new StreamReader(stream);
            var typescriptServicesSource = reader.ReadToEnd();

            Native.JsCreateRuntime(JavaScriptRuntimeAttributes.None, null, out _runtime);

            Native.JsCreateContext(_runtime, out _context);

            Native.JsSetCurrentContext(_context);

            // Install the compiler in the context
            Native.JsRunScript(typescriptServicesSource, _currentSourceContext++, string.Empty, out _);

            Native.JsSetCurrentContext(JavaScriptContext.Invalid);
        }

        public string Transpile(string input)
        {
            lock (this)
            {
                Native.JsSetCurrentContext(_context);

                Native.JsRunScript(@"
(input) => {
    const result = ts.transpileModule(input, {
        compilerOptions: {
            target: ts.ScriptTarget.ES2015,
            module: ts.ModuleKind.ES2015,
            lib: [ 'ES2015' ]
        }
    });
    return result.outputText;
}", _currentSourceContext++, string.Empty, out var transpileFunction);

                var resultValue = transpileFunction.CallFunction(JavaScriptValue.GlobalObject, JavaScriptValue.FromString(input));

                Native.JsStringToPointer(resultValue, out var resultPtr, out var stringLength);
                var outputText = Marshal.PtrToStringUni(resultPtr);

                Native.JsSetCurrentContext(JavaScriptContext.Invalid);

                return outputText;
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                Native.JsDisposeRuntime(_runtime);
            }
        }
    }
}
