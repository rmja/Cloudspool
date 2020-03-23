using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Api.Infrastructure
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
    {
        private readonly CloudspoolContext _db;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            CloudspoolContext db) : base(options, logger, encoder, clock)
        {
            _db = db;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var headerValue))
            {
                return AuthenticateResult.NoResult();
            }

            if (!TryParseAuthorizationValue(headerValue, out var key))
            {
                return AuthenticateResult.NoResult();
            }

            var project = await _db.Project.SingleOrDefaultAsync(x => x.Key == key);

            if (project is null)
            {
                return AuthenticateResult.Fail("Invalid key");
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Project"),
                new Claim("ProjectId", project.Id.ToString())
            }, Options.AuthenticationType);
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

        private static bool TryParseAuthorizationValue(string headerValue, out Guid key)
        {
            var prefix = "Bearer project:";
            if (headerValue.StartsWith(prefix) && Guid.TryParse(headerValue.AsSpan(prefix.Length), out key))
            {
                return true;
            }

            key = default;
            return false;
        }
    }
}
