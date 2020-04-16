using Cloudspool.Api.Client;
using Refit;
using System;
using System.Net.Http;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiClientExtensions
    {
        public static IHttpClientBuilder AddCloudspoolApiClient(this IServiceCollection services, Action<ApiClientOptions> configure)
        {
            return services
                .Configure(configure)
                .AddTransient<AuthorizationMessageHandler>()
                .AddTransient<RedirectMessageHandler>()
                .AddRefitClient<IApiClient>(new RefitSettings(new SystemTextJsonContentSerializer(new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { AllowAutoRedirect = false })
                .AddHttpMessageHandler<AuthorizationMessageHandler>()
                .AddHttpMessageHandler<RedirectMessageHandler>();
        }
    }
}
