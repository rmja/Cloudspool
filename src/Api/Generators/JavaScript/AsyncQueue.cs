using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Generators.JavaScript
{
    public class AsyncQueue<T> : IDisposable
    {
        private readonly SemaphoreSlim _count = new SemaphoreSlim(initialCount: 0);
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            _count.Release();
        }

        public void EnqueueRange(IEnumerable<T> items)
        {
            var count = 0;
            foreach (var item in items)
            {
                _queue.Enqueue(item);
                count++;
            }
            _count.Release(count);
        }

        public T Dequeue()
        {
            _count.Wait();

            Debug.Assert(_queue.TryDequeue(out var item) == true);
            return item;
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _count.WaitAsync(cancellationToken);

            Debug.Assert(_queue.TryDequeue(out var item) == true);
            return item;
        }

        public void Dispose()
        {
            _count.Dispose();
        }
    }
}
