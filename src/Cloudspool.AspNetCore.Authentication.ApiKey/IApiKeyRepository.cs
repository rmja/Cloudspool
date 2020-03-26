using System.Threading.Tasks;

namespace Cloudspool.AspNetCore.Authentication.ApiKey
{
    public interface IApiKeyRepository
    {
        Task<ApiKey> GetByKey(string key);
    }
}
