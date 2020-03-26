using System;
using System.Threading.Tasks;

namespace Cloudspool.Api.Client
{
    public class ApiClientOptions
    {
        public Func<IServiceProvider, Task<string>> GetApiKeyAsync { get; set; }
    }
}
