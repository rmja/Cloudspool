using System;
using System.Threading.Tasks;

namespace Api.Client
{
    public class ApiClientOptions
    {
        public Func<IServiceProvider, Task<string>> GetApiKeyAsync { get; set; }
    }
}
