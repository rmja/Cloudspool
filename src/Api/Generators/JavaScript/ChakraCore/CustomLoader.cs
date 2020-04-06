using ChakraCore.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Api.Generators.JavaScript.ChakraCore
{
    class CustomLoader : IDisposable
    {
        private readonly JavaScriptContext _context;
        private readonly ScriptDispatcher _dispatcher;
        private readonly ResourceScriptFactory _resourceScriptFactory;
        private readonly IResourceManager _resourceManager;
        private readonly Dictionary<string, ModuleLease> _moduleLeases = new Dictionary<string, ModuleLease>();

        public FetchImportedModuleCallBack LoadModule { get; }
        public NotifyModuleReadyCallback ModuleLoaded { get; }
        public JavaScriptModuleRecord RootModule { get; set; }
        public Action OnRootModuleEvaluated { get; set; }
        public Action<Exception> OnResourceLoadError { get; set; }

        public CustomLoader(JavaScriptContext context, ScriptDispatcher dispatcher, ResourceScriptFactory resourceScriptFactory, IResourceManager resourceManager)
        {
            _context = context;
            _dispatcher = dispatcher;
            _resourceScriptFactory = resourceScriptFactory;
            _resourceManager = resourceManager;

            // Store the callbacks as delegate to avoid that they are moved around
            LoadModule = LoadModuleImpl;
            ModuleLoaded = ModuleLoadedImpl;
        }

        public void AddPreload(string specifier, string code)
        {
            if (!RootModule.IsValid)
            {
                throw new InvalidOperationException("No root module set");
            }

            var module = JavaScriptModuleRecord.Initialize(RootModule, specifier);
            module.HostUrl = specifier; // Only for debugging
            _moduleLeases.Add(specifier, new ModuleLease(module));

            _dispatcher.Invoke(() =>
            {
                using (_context.GetScope())
                {
                    module.ParseSource(code);
                }
            });
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
                dependentModuleRecord = JavaScriptModuleRecord.Initialize(referencingModule, specifier);
                dependentModuleRecord.HostUrl = specifierString; // Only for debugging
                dependentModuleRecord.Exception = JavaScriptValue.CreateTypeError($"Failed to resolve module for specifier '{specifierString}'");
                _moduleLeases.Add(specifierString, new ModuleLease(dependentModuleRecord));
                dependentModuleRecord = _moduleLeases[specifierString].Module;
                return JavaScriptErrorCode.NoError;
            }

            dependentModuleRecord = JavaScriptModuleRecord.Initialize(referencingModule, specifier);
            dependentModuleRecord.HostUrl = specifierString; // Only for debugging
            _moduleLeases.Add(specifierString, new ModuleLease(dependentModuleRecord));

            // Fire off a task in the threadpool
            Task.Run(async () =>
            {
                var module = _moduleLeases[specifierString].Module;
                try
                {
                    var resource = await _resourceManager.GetResourceAsync(alias);

                    if (resource is object)
                    {
                        var script = _resourceScriptFactory.CreateFromExtension(resource, Path.GetExtension(specifierString));

                        _dispatcher.Invoke(() =>
                        {
                            using (_context.GetScope())
                            {
                                module.ParseSource(script);
                            }
                        });
                    }
                    else
                    {
                        _dispatcher.Invoke(() =>
                        {
                            using (_context.GetScope())
                            {
                                ThrowModuleException(module, JavaScriptValue.CreateError($"Could not find the resource '{specifierString}'"));
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    _dispatcher.Invoke(() =>
                    {
                        using (_context.GetScope())
                        {
                            ThrowModuleException(module, JavaScriptValue.CreateError(e.Message));
                        }
                    });
                }

                void ThrowModuleException(JavaScriptModuleRecord module, JavaScriptValue error)
                {
                    error.AddRef();
                    module.Exception = error;

                    if (!JavaScriptContext.HasException)
                    {
                        JavaScriptContext.SetException(error);
                    }

                    try
                    {
                        Native.ThrowIfError(JavaScriptErrorCode.ScriptException);
                    }
                    catch (Exception e)
                    {
                        OnResourceLoadError?.Invoke(e);
                    }
                    finally
                    {
                        error.Release();
                    }
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
                _dispatcher.Invoke(() =>
                {
                    using (_context.GetScope())
                    {
                        referencingModule.Evaluate();

                        if (referencingModule.Equals(RootModule))
                        {
                            OnRootModuleEvaluated?.Invoke();
                        }
                    }
                });
            }

            return JavaScriptErrorCode.NoError;
        }

        public void Dispose()
        {
            using (_context.GetScope())
            {
                foreach (var module in _moduleLeases.Values)
                {
                    module.Dispose();
                }
            }
        }
    }
}
