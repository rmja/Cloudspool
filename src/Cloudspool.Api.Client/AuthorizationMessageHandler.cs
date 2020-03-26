using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Cloudspool.Api.Client
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IServiceProvider _services;
        private readonly ApiClientOptions _options;

        public AuthorizationMessageHandler(IServiceProvider services, IOptions<ApiClientOptions> options)
        {
            _services = services;
            _options = options.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var auth = request.Headers.Authorization;

            if (auth is object)
            {
                var apiKey = await _options.GetApiKeyAsync(_services);
                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, apiKey);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
