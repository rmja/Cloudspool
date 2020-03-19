using Api.Client;
using Refit;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiExtensions
    {
        public static IHttpClientBuilder AddApiClient(this IServiceCollection services)
        {
            return services.AddRefitClient<IApiClient>();
        }
    }
}
