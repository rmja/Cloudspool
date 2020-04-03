using Api.Generators;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Tests.Generators
{
    class DictionaryResourceManager : Dictionary<string, byte[]>, IResourceManager
    {
        public TimeSpan AsyncLoadDelay { get; set; } = TimeSpan.Zero;

        public byte[] GetResource(string alias)
        {
            return TryGetValue(alias, out var resource) ? resource : null;
        }

        public async Task<byte[]> GetResourceAsync(string alias, CancellationToken cancellationToken = default)
        {
            if (AsyncLoadDelay != TimeSpan.Zero)
            {
                await Task.Delay(AsyncLoadDelay);
            }

            var resource = GetResource(alias);
            return resource;
        }
    }
}
