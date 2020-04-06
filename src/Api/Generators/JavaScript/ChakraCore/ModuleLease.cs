using ChakraCore.API;
using System;

namespace Api.Generators.JavaScript.ChakraCore
{
    public class ModuleLease : IDisposable
    {
        private bool _disposed = false;
        public JavaScriptModuleRecord Module { get; }

        public ModuleLease(JavaScriptModuleRecord module)
        {
            Module = module;

            // Make sure that the JavaScript GC does not collect the module, see
            // https://github.com/microsoft/ChakraCore/wiki/JavaScript-Runtime-(JSRT)-Overview#memory-management
            // https://github.com/Taritsyn/TestChakraCoreEsModules/blob/master/TestChakraCoreEsModules/EsModuleManager.cs#L92
            Module.AddRef();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Module.Release();
            _disposed = true;
        }
    }
}
