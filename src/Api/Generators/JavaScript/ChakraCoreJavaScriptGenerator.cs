using Api.Generators.JavaScript.Polyfills;
using ChakraCore.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Generators.JavaScript
{
    // https://github.com/microsoft/ChakraCore/wiki/JavaScript-Runtime-(JSRT)-Overview
    // https://github.com/Taritsyn/TestChakraCoreEsModules/blob/master/TestChakraCoreEsModules/EsModuleManager.cs
    // What’s needed to make dynamic import work? https://github.com/Microsoft/ChakraCore/issues/3793
    // Correct FetchImportedModuleFromScriptCallBack comments in header https://github.com/microsoft/ChakraCore/pull/4581/files
    // Steps needed for es6 modules embedding ChakraCore https://github.com/microsoft/ChakraCore/issues/4324
    // See https://blogs.windows.com/msedgedev/2015/05/18/using-chakra-for-scripting-applications-across-windows-10/
    public class ChakraCoreJavaScriptGenerator : IJavaScriptGenerator
    {
        private readonly ResourceScriptFactory _resourceScriptFactory;
        private static readonly Action CompleteAdding = new Action(() => { });

        public ChakraCoreJavaScriptGenerator(ResourceScriptFactory resourceScriptFactory)
        {
            _resourceScriptFactory = resourceScriptFactory;
        }

        public string[] ValidateTemplate(string code)
        {
            using var runtime = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.EnableExperimentalFeatures);
            var context = JavaScriptContext.CreateContext(runtime);
            context.AddRef();

            try
            {
                using (var scope = context.GetScope())
                {
                    try
                    {
                        JavaScriptContext.RunScript("const globalThis = this;");
                        JavaScriptContext.RunScript(PolyfillScripts.Get("ImageData"));

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
            }
            finally
            {
                context.Release();
            }
        }

        public async Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null, CancellationToken cancellationToken = default)
        {
            using var runtime = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.EnableExperimentalFeatures);
            var context = JavaScriptContext.CreateContext(runtime);

            // Make sure that the JavaScript GC does not collect the context
            // We need to do this, as we throughout the code sets
            //   JavaScriptContext.Current = JavaScriptContext.Invalid
            // implicitly everytime we dispose a scope. This action allows the JavaScript GC to dispose the context,
            // and to hinder that, we increment the ref counter. The counter is decremented in the finally block.
            context.AddRef();

            try
            {
                using var taskQueue = new AsyncQueue<Action>();
                using var loader = new CustomLoader(context, taskQueue, _resourceScriptFactory, resourceManager);
                JavaScriptValue result = default;
                Exception exception = null;

                // Start a scope on the current context
                // A scope effectively ensures that JavaScriptContext.Current = context
                // until the scope is disposed.
                // We use multiple scopes, explicitly in the task queue below, as we cannot guarantee that
                // the continuation after "await" is on the same thread. We therefore start a new scope after "await"'s.
                using (var scope = context.GetScope())
                {
                    Native.ThrowIfError(Native.JsSetPromiseContinuationCallback((task, callbackState) =>
                    {
                        // Make sure that the continuation task is not free'd by the JavaScript GC, see
                        // https://github.com/microsoft/ChakraCore/wiki/JavaScript-Runtime-(JSRT)-Overview#promises
                        task.AddRef();

                        taskQueue.Enqueue(() =>
                        {
                            task.CallFunction(task);

                            // The task is completed, allow the JavaScript GC to free it.
                            task.Release();
                        });
                    }, IntPtr.Zero));

                    // Load polyfill's
                    JavaScriptContext.RunScript("const globalThis = this;");
                    JavaScriptContext.RunScript(PolyfillScripts.Get("ImageData"));

                    var rootModule = JavaScriptModuleRecord.Initialize();
                    loader.RootModule = rootModule;
                    rootModule.FetchImportedModuleCallBack = loader.LoadModule;
                    rootModule.NotifyModuleReadyCallback = loader.ModuleLoaded;

                    loader.AddPreload(referencingModule: rootModule, "main", code);

                    rootModule.ParseSource(@"
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

                    // Add callback that will be run when the root module has been evaluated,
                    // at which time its rootModule.Namespace becomes valid.
                    loader.RootModuleEvaluated = () => taskQueue.Enqueue(() =>
                    {
                        result = JavaScriptValue.Undefined;

                        var build = rootModule.Namespace.GetProperty("build");
                        var modelJson = JavaScriptValue.FromString(JsonSerializer.Serialize(model));
                        var resultPromise = build.CallFunction(JavaScriptValue.GlobalObject, modelJson);
                        
                        resultPromise
                            .GetProperty("then").CallFunction(resultPromise, JavaScriptValue.CreateFunction((callee, isConstructCall, arguments, argumentCount, callbackState) =>
                            {
                                result = arguments[1];
                                taskQueue.Enqueue(CompleteAdding);
                                return callee;
                            }))
                            .GetProperty("catch").CallFunction(resultPromise, JavaScriptValue.CreateFunction((callee, isConstructCall, arguments, argumentCount, callbackState) =>
                            {
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
                                taskQueue.Enqueue(CompleteAdding);
                                return callee;
                            }));
                    });
                }

                // Run the task queue
                while (true)
                {
                    var action = await taskQueue.DequeueAsync();

                    if (action == CompleteAdding)
                    {
                        break;
                    }

                    // We cannot quarantee that we are on the same thread here as before we await'ed.
                    // We therefore set the context to the current thread by starting a new scope.
                    using (var scope = context.GetScope())
                    {
                        action();
                    }
                }

                using (var scope = context.GetScope())
                {
                    loader.ThrowIfModulesFailedToLoad();

                    if (exception is object)
                    {
                        throw exception;
                    }

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
            }
            finally
            {
                context.Release();
            }
        }

        class CustomLoader : IDisposable
        {
            private readonly JavaScriptContext _context;
            private readonly AsyncQueue<Action> _taskQueue;
            private readonly ResourceScriptFactory _resourceScriptFactory;
            private readonly IResourceManager _resourceManager;
            private readonly Dictionary<string, ModuleLease> _moduleLeases = new Dictionary<string, ModuleLease>();

            public FetchImportedModuleCallBack LoadModule { get; }
            public NotifyModuleReadyCallback ModuleLoaded { get; }
            public JavaScriptModuleRecord RootModule { get; set; }
            public Action RootModuleEvaluated { get; set; }

            public CustomLoader(JavaScriptContext context, AsyncQueue<Action> taskQueue, ResourceScriptFactory resourceScriptFactory, IResourceManager resourceManager)
            {
                _context = context;
                _taskQueue = taskQueue;
                _resourceScriptFactory = resourceScriptFactory;
                _resourceManager = resourceManager;

                // Store the callbacks as delegate to avoid that they are moved around
                LoadModule = LoadModuleImpl;
                ModuleLoaded = ModuleLoadedImpl;
            }

            public void AddPreload(JavaScriptModuleRecord referencingModule, string specifier, string code)
            {
                var module = JavaScriptModuleRecord.Initialize(referencingModule, specifier);
                module.HostUrl = specifier; // Only for debugging
                _moduleLeases.Add(specifier, new ModuleLease(module));

                _taskQueue.Enqueue(() => module.ParseSource(code));
            }

            /// <summary>
            ///     User implemented callback to fetch additional imported modules in ES modules.
            /// </summary>
            /// <remarks>
            ///     The callback is invoked on the current runtime execution thread, therefore execution is blocked until
            ///     the callback completes. Notify the host to fetch the dependent module. This is the "import" part
            ///     before HostResolveImportedModule in ES6 spec. This notifies the host that the referencing module has
            ///     the specified module dependency, and the host needs to retrieve the module back.
            ///
            ///     Callback should:
            ///     1. Check if the requested module has been requested before - if yes return the existing
            ///         module record
            ///     2. If no create and initialize a new module record with JsInitializeModuleRecord to
            ///         return and schedule a call to JsParseModuleSource for the new record.
            /// </remarks>
            /// <param name="referencingModule">The referencing module that is requesting the dependent module.</param>
            /// <param name="specifier">The specifier coming from the module source code.</param>
            /// <param name="dependentModuleRecord">The ModuleRecord of the dependent module. If the module was requested
            ///                                     before from other source, return the existing ModuleRecord, otherwise
            ///                                     return a newly created ModuleRecord.</param>
            /// <returns>
            ///     Returns a <c>JsNoError</c> if the operation succeeded an error code otherwise.
            /// </returns>
            /// <see cref="JavaScriptFetchImportedModuleCallBack"/>
            private JavaScriptErrorCode LoadModuleImpl(JavaScriptModuleRecord referencingModule, JavaScriptValue specifier, out JavaScriptModuleRecord dependentModuleRecord)
            {
                var specifierString = specifier.ToString();

                if (_moduleLeases.TryGetValue(specifierString, out var lease))
                {
                    dependentModuleRecord = lease.Module;
                    return JavaScriptErrorCode.NoError;
                }

                var alias = ResourceModuleUtils.GetResourceAlias(specifierString);

                if (alias is null)
                {
                    dependentModuleRecord = JavaScriptModuleRecord.Invalid;
                    return JavaScriptErrorCode.NoError;
                }

                var module = JavaScriptModuleRecord.Initialize(referencingModule, specifier);
                _moduleLeases.Add(specifierString, new ModuleLease(module));
                dependentModuleRecord = module;

                // Fire off a task in the threadpool to and insert the result in the taskqueue when when done
                Task.Run(async () =>
                {
                    try
                    {
                        var resource = await _resourceManager.GetResourceAsync(alias);

                        if (resource is object)
                        {
                            var script = _resourceScriptFactory.CreateFromExtension(resource, Path.GetExtension(specifierString));

                            _taskQueue.Enqueue(() =>
                            {
                                module.ParseSource(script);
                            });
                        }
                        else
                        {
                            _taskQueue.Enqueue(() =>
                            {
                                module.Exception = JavaScriptValue.CreateError($"Could not find the resource '{specifierString}'");
                                _taskQueue.Enqueue(CompleteAdding);
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        _taskQueue.Enqueue(() =>
                        {
                            module.Exception = JavaScriptValue.CreateError(e.Message);
                        });
                    }
                });

                return JavaScriptErrorCode.NoError;
            }

            /// <summary>
            ///     User implemented callback to get notification when the module is ready.
            /// </summary>
            /// <remarks>
            ///     The callback is invoked on the current runtime execution thread, therefore execution is blocked until the
            ///     callback completes. This callback should schedule a call to JsEvaluateModule to run the module that has been loaded.
            /// </remarks>
            /// <param name="referencingModule">The referencing module that has finished running ModuleDeclarationInstantiation step.</param>
            /// <param name="exceptionVar">If nullptr, the module is successfully initialized and host should queue the execution job
            ///                            otherwise it's the exception object.</param>
            /// <returns>
            ///     Returns a JsErrorCode - note, the return value is ignored.
            /// </returns>
            /// <see cref="NotifyModuleReadyCallback"></see>
            private JavaScriptErrorCode ModuleLoadedImpl(JavaScriptModuleRecord referencingModule, JavaScriptValue exceptionVar)
            {
                if (!exceptionVar.IsValid)
                {
                    _taskQueue.Enqueue(() =>
                    {
                        referencingModule.Evaluate();

                        if (referencingModule.Equals(RootModule))
                        {
                            RootModuleEvaluated?.Invoke();
                        }
                    });
                }

                return JavaScriptErrorCode.NoError;
            }

            public void ThrowIfModulesFailedToLoad()
            {
                foreach (var module in _moduleLeases.Values)
                {
                    if (module.Module.Exception.IsValid)
                    {
                        if (!JavaScriptContext.HasException)
                        {
                            JavaScriptContext.SetException(module.Module.Exception);
                        }

                        Native.ThrowIfError(JavaScriptErrorCode.ScriptException);
                    }
                }
            }

            public void Dispose()
            {
                using (var scope = _context.GetScope())
                {
                    foreach (var module in _moduleLeases.Values)
                    {
                        module.Dispose();
                    }
                }
            }

            class ModuleLease : IDisposable
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
                    if (!_disposed)
                    {
                        Module.Release();
                        _disposed = true;
                    }
                }
            }
        }
    }
}
