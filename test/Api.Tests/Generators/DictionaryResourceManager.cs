using Api.Generators;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Tests.Generators
{
    class DictionaryResourceManager : Dictionary<string, byte[]>, IResourceManager
    {
        public byte[] GetResource(string alias)
        {
            return TryGetValue(alias, out var resource) ? resource : null;
        }

        public async Task<byte[]> GetResourceAsync(string alias, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100);
            var resource = GetResource(alias);
            return resource;
        }
    }
}
