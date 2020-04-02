using System.Threading;
using System.Threading.Tasks;

namespace Api.Generators
{
    public interface IResourceManager
    {
        byte[] GetResource(string alias);
        Task<byte[]> GetResourceAsync(string alias, CancellationToken cancellationToken = default);
    }
}
