using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Api.Generators.JavaScript.ChakraCore
{
    public class ScriptDispatcher : IDisposable
    {
        private bool _isDisposed = false;
        private Thread _thread;
        private BlockingCollection<IScriptTask> _queue = new BlockingCollection<IScriptTask>();

        public ScriptDispatcher()
        {
            _thread = new Thread(Run, ChakraCoreSettings.MaxStackSize);
            _thread.Start();
        }

        public void Invoke(Action action)
        {
            if (Thread.CurrentThread == _thread)
            {
                action();
                return;
            }

            using (var task = new ScriptTask(action))
            {
                ExecuteTask(task);
            }
        }

        public T Invoke<T>(Func<T> func)
        {
            if (Thread.CurrentThread == _thread)
            {
                return func();
            }

            using (var task = new ScriptTask<T>(func))
            {
                ExecuteTask(task);
                return task.Result;
            }
        }

        private void ExecuteTask(IScriptTask task)
        {
            _queue.Add(task);
            task.WaitForCompletion();

            if (task.Exception is object)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }
        }

        private void Run()
        {
            foreach (var job in _queue.GetConsumingEnumerable())
            {
                job.Run();
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _queue.CompleteAdding();
                _thread.Join();
                _thread = null;
                _queue.Dispose();
                _queue = null;
                _isDisposed = true;
            }
        }

        private interface IScriptTask
        {
            Exception Exception { get; }
            void Run();
            void WaitForCompletion();
        }

        private class ScriptTask : IScriptTask, IDisposable
        {
            private readonly Action _action;
            private readonly ManualResetEvent _doneHandle = new ManualResetEvent(false);

            public Exception Exception { get; set; }

            public ScriptTask(Action action)
            {
                _action = action;
            }

            public void Run()
            {
                try
                {
                    _action();
                }
                catch (Exception e)
                {
                    Exception = e;
                }

                _doneHandle.Set();
            }

            public void WaitForCompletion()
            {
                _doneHandle.WaitOne();
            }

            public void Dispose()
            {
                _doneHandle.Dispose();
            }
        }

        private class ScriptTask<T> : IScriptTask, IDisposable
        {
            private readonly Func<T> _func;
            private readonly ManualResetEvent _doneHandle = new ManualResetEvent(false);

            public T Result { get; set; }
            public Exception Exception { get; set; }

            public ScriptTask(Func<T> func)
            {
                _func = func;
            }

            public void Run()
            {
                try
                {
                    Result = _func();
                }
                catch (Exception e)
                {
                    Exception = e;
                }

                _doneHandle.Set();
            }

            public void WaitForCompletion()
            {
                _doneHandle.WaitOne();
            }

            public void Dispose()
            {
                _doneHandle.Dispose();
            }
        }
    }
}
