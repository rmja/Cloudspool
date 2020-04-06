using Api.Generators.JavaScript.Polyfills;
using ChakraCore.API;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Generators.JavaScript.ChakraCore
{
    // https://github.com/microsoft/ChakraCore/wiki/JavaScript-Runtime-(JSRT)-Overview
    // https://github.com/Taritsyn/JavaScriptEngineSwitcher/blob/master/src/JavaScriptEngineSwitcher.ChakraCore/ChakraCoreJsEngine.cs
    // https://github.com/Taritsyn/TestChakraCoreEsModules/blob/master/TestChakraCoreEsModules/EsModuleManager.cs
    // What’s needed to make dynamic import work? https://github.com/Microsoft/ChakraCore/issues/3793
    // Correct FetchImportedModuleFromScriptCallBack comments in header https://github.com/microsoft/ChakraCore/pull/4581/files
    // Steps needed for es6 modules embedding ChakraCore https://github.com/microsoft/ChakraCore/issues/4324
    // See https://blogs.windows.com/msedgedev/2015/05/18/using-chakra-for-scripting-applications-across-windows-10/
    public class ChakraCoreJavaScriptGenerator : IJavaScriptGenerator, IDisposable
    {
        private readonly ResourceScriptFactory _resourceScriptFactory;
        private readonly JavaScriptPromiseContinuationCallback _promiseContinuationCallback;
        private readonly ScriptDispatcher _dispatcher = new ScriptDispatcher();
        private JavaScriptRuntime _runtime;

        public ChakraCoreJavaScriptGenerator(ResourceScriptFactory resourceScriptFactory)
        {
            _resourceScriptFactory = resourceScriptFactory;
            _promiseContinuationCallback = PromiseContinuationCallback;

            _dispatcher.Invoke(() =>
            {
                _runtime = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.EnableExperimentalFeatures);
                _runtime.MemoryLimit = ChakraCoreSettings.MemoryLimit;
            });
        }

        /// <summary>
        ///     A promise continuation callback.
        /// </summary>
        /// <remarks>
        ///     The host can specify a promise continuation callback in <c>JsSetPromiseContinuationCallback</c>. If
        ///     a script creates a task to be run later, then the promise continuation callback will be called with
        ///     the task and the task should be put in a FIFO queue, to be run when the current script is
        ///     done executing.
        /// </remarks>
        /// <param name="task">The task, represented as a JavaScript function.</param>
        /// <param name="callbackState">The data argument to be passed to the callback.</param>
        /// <see cref="JavaScriptPromiseContinuationCallback"></see>
        private void PromiseContinuationCallback(JavaScriptValue task, IntPtr callbackState)
        {
            task.AddRef();

            try
            {
                task.CallFunction(JavaScriptValue.GlobalObject);
            }
            finally
            {
                task.Release();
            }
        }

        public string[] ValidateTemplate(string code)
        {
            var context = JavaScriptContext.CreateContext(_runtime);
            context.AddRef();

            try
            {
                return _dispatcher.Invoke(() =>
                {
                    using (context.GetScope())
                    {
                        Native.ThrowIfError(Native.JsSetPromiseContinuationCallback(_promiseContinuationCallback, IntPtr.Zero));

                        // Load polyfill's
                        JavaScriptContext.RunScript("const globalThis = this;");
                        JavaScriptContext.RunScript(PolyfillScripts.Get("ImageData"));

                        try
                        {
                            var module = JavaScriptModuleRecord.Initialize();
                            module.FetchImportedModuleCallBack = (JavaScriptModuleRecord referencingModule, JavaScriptValue specifier, out JavaScriptModuleRecord dependentModuleRecord) =>
                            {
                                dependentModuleRecord = JavaScriptModuleRecord.Invalid;
                                return JavaScriptErrorCode.NoError;
                            };
                            module.NotifyModuleReadyCallback = (JavaScriptModuleRecord referencingModule, JavaScriptValue exceptionVar) => JavaScriptErrorCode.NoError;
                            module.ParseSource(code);

                            return Array.Empty<string>();
                        }
                        catch (JavaScriptException e) when (e.ErrorCode == JavaScriptErrorCode.ScriptCompile)
                        {
                            return new[] { e.Message };
                        }
                    }
                });
            }
            finally
            {
                context.Release();
            }
        }

        public async Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null, CancellationToken cancellationToken = default)
        {
            // Multiple contexts can share a runtime, but only one thread can access the runtime at a time
            var context = JavaScriptContext.CreateContext(_runtime);
            context.AddRef();
            ModuleLease rootModuleLease = null;

            try
            {
                using var loader = new CustomLoader(context, _dispatcher, _resourceScriptFactory, resourceManager);
                var tcs = new TaskCompletionSource<JavaScriptValue>(TaskCreationOptions.RunContinuationsAsynchronously);

                _dispatcher.Invoke(() =>
                {
                    using (context.GetScope())
                    {
                        Native.ThrowIfError(Native.JsSetPromiseContinuationCallback(_promiseContinuationCallback, IntPtr.Zero));

                        // Load polyfill's
                        JavaScriptContext.RunScript("const globalThis = this;");
                        JavaScriptContext.RunScript(PolyfillScripts.Get("ImageData"));

                        var rootModule = JavaScriptModuleRecord.Initialize();
                        rootModule.AddRef();
                        rootModule.HostUrl = "<host>"; // Only for debugging

                        loader.RootModule = rootModule;
                        rootModule.FetchImportedModuleCallBack = loader.LoadModule;
                        rootModule.NotifyModuleReadyCallback = loader.ModuleLoaded;
                        rootModuleLease = new ModuleLease(rootModule);

                        loader.AddPreload("main", code);
                    }
                });

                // Add callback that will be run when the root module has been evaluated,
                // at which time its rootModule.Namespace becomes valid.
                loader.OnRootModuleEvaluated = () =>
                {
                    using (context.GetScope())
                    {
                        var build = rootModuleLease.Module.Namespace.GetProperty("build");
                        build.AddRef();
                        
                        var modelJson = JavaScriptValue.FromString(JsonSerializer.Serialize(model));
                        modelJson.AddRef();
                        
                        var resultPromise = build.CallFunction(JavaScriptValue.GlobalObject, modelJson);
                        resultPromise.AddRef();

                        FunctionLease resolve = default;
                        FunctionLease reject = default;
                        FunctionLease always = default;

                        resolve = new FunctionLease((callee, isConstructCall, arguments, argumentCount, callbackState) =>
                        {
                            var result = arguments[1];

                            result.AddRef();
                            tcs.SetResult(result);

                            return callee;
                        });

                        reject = new FunctionLease((callee, isConstructCall, arguments, argumentCount, callbackState) =>
                        {
                            JavaScriptException exception;

                            if (arguments.Length >= 2)
                            {
                                var reason = arguments[1];
                                if (reason.ValueType == JavaScriptValueType.Error)
                                {
                                    exception = new JavaScriptException(JavaScriptErrorCode.ScriptException, reason.GetProperty("message").ToString());
                                }
                                else
                                {
                                    exception = new JavaScriptException(JavaScriptErrorCode.ScriptException, reason.ConvertToString().ToString());
                                }
                            }
                            else
                            {
                                exception = new JavaScriptException(JavaScriptErrorCode.ScriptException);
                            }

                            tcs.SetException(exception);

                            return callee;
                        });

                        always = new FunctionLease((callee, isConstructCall, arguments, argumentCount, callbackState) =>
                        {
                            build.Release();
                            modelJson.Release();
                            resultPromise.Release();

                            resolve.Dispose();
                            reject.Dispose();
                            always.Dispose();

                            return callee;
                        });

                        resultPromise
                            .GetProperty("then").CallFunction(resultPromise, resolve.Function)
                            .GetProperty("catch").CallFunction(resultPromise, reject.Function)
                            .GetProperty("finally").CallFunction(resultPromise, always.Function);
                    }
                };
                loader.OnResourceLoadError = e => tcs.SetException(e);

                _dispatcher.Invoke(() =>
                {
                    using (context.GetScope())
                    {
                        rootModuleLease.Module.ParseSource(@"
import Builder from 'main';

const builder = new Builder();
const build = async (modelJson) => {
const model = JSON.parse(modelJson);
const content = await Promise.resolve(builder.build(model));
try {
const { contentType } = await import('main');
return { content, contentType };
} catch (error) {
return { content };
}
};

export { build };
");
                    }
                });

                var result = await tcs.Task;

                return _dispatcher.Invoke(() =>
                {
                    using (context.GetScope())
                    {
                        _runtime.CollectGarbage();

                        var content = result.GetProperty("content");
                        var contentTypeValue = result.GetProperty("contentType");
                        var contentType = contentTypeValue.ValueType == JavaScriptValueType.String ? contentTypeValue.ToString() : null;

                        switch (content.ValueType)
                        {
                            case JavaScriptValueType.String: return new GenerateResult() { Content = Encoding.UTF8.GetBytes(content.ToString()), ContentType = contentType ?? "text/plain" };
                            case JavaScriptValueType.TypedArray:
                                {
                                    content.GetTypedArrayInfo(out var arrayType, out var buffer, out var byteOffset, out var byteLength);

                                    if (arrayType != JavaScriptTypedArrayType.Uint8)
                                    {
                                        throw new NotSupportedException("Typed array must be Uint8Array");
                                    }

                                    var bufferPointer = buffer.GetArrayBufferStorage(out var bufferLength);
                                    var array = new byte[byteLength];
                                    Marshal.Copy(IntPtr.Add(bufferPointer, (int)byteOffset), array, 0, (int)byteLength);

                                    return new GenerateResult() { Content = array, ContentType = contentType ?? "application/octet-stream" };
                                }
                            case JavaScriptValueType.Array:
                                {
                                    var list = content.ToList();
                                    var array = new byte[list.Count];
                                    for (var i = 0; i < list.Count; i++)
                                    {
                                        array[i] = (byte)list[i].ToInt32();
                                    }

                                    return new GenerateResult() { Content = array, ContentType = contentType ?? "application/octet-stream" };
                                }
                            default: throw new NotSupportedException("Build did not produce a supported result");
                        }
                    }
                });
            }
            finally
            {
                _dispatcher.Invoke(() =>
                {
                    using (context.GetScope())
                    {
                        rootModuleLease?.Dispose();
                    }
                });
                context.Release();
            }
        }

        public void Dispose()
        {
            _runtime.Dispose();
        }
    }
}
