using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cloudspool.AspNetCore.Authentication.ApiKey
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
    {
        private readonly IApiKeyRepository _keyRepository;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyRepository keyRepository) : base(options, logger, encoder, clock)
        {
            _keyRepository = keyRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var headerValue))
            {
                return AuthenticateResult.NoResult();
            }

            if (!TryGetKey(headerValue, out var key))
            {
                return AuthenticateResult.NoResult();
            }

            var apiKey = await _keyRepository.GetByKey(key);

            if (apiKey is null)
            {
                return AuthenticateResult.Fail("Invalid key");
            }

            var identity = new ClaimsIdentity(apiKey.Claims, Options.AuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Options.AuthenticationScheme);

            return AuthenticateResult.Success(ticket);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        private static bool TryGetKey(string headerValue, out string key)
        {
            var prefix = "Bearer ";
            if (headerValue.StartsWith(prefix))
            {
                key = headerValue.Substring(prefix.Length);
                return true;
            }

            key = default;
            return false;
        }
    }
}
