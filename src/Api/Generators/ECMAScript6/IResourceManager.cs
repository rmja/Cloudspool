using System.Threading;
using System.Threading.Tasks;

namespace Api.Generators.ECMAScript6
{
    public interface IResourceManager
    {
        byte[] GetResource(string alias);
        Task<byte[]> GetResourceAsync(string alias, CancellationToken cancellationToken);
    }
}
