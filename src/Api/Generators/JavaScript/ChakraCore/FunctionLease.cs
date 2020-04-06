using ChakraCore.API;
using System;
using System.Runtime.InteropServices;

namespace Api.Generators.JavaScript.ChakraCore
{
    public class FunctionLease : IDisposable
    {
        private bool _disposed = false;
        private readonly GCHandle _handle;

        public JavaScriptValue Function { get; }
        public FunctionLease(JavaScriptNativeFunction function)
        {
            Function = JavaScriptValue.CreateFunction(function);
            Function.AddRef();
            _handle = GCHandle.Alloc(function);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Function.Release();
            _handle.Free();
            _disposed = true;
        }
    }
}
