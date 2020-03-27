﻿using Cloudspool.Api.Client;
using Refit;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiClientExtensions
    {
        public static IHttpClientBuilder AddCloudspoolApiClient(this IServiceCollection services, Action<ApiClientOptions> configure)
        {
            return services
                .Configure(configure)
                .AddScoped<AuthorizationMessageHandler>()
                .AddRefitClient<IApiClient>()
                .AddHttpMessageHandler<AuthorizationMessageHandler>();
        }
    }
}