using Api.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dispatcher
{
	public class SpoolerKeyAuthorizationMiddleware : IMiddleware
    {
		private readonly IApiClient _api;

		public SpoolerKeyAuthorizationMiddleware(IApiClient api)
		{
			_api = api;
		}

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
			if (!context.User.Identity.IsAuthenticated)
			{
				if (context.Request.Headers.TryGetValue(HeaderNames.Authorization, out var headerValue) && TryParseAuthorizationValue(headerValue, out var spoolerKey))
				{
					var spooler = await _api.SpoolerGetByKey(spoolerKey);
					var identity = new ClaimsIdentity(new[]
						{
							new Claim(ClaimTypes.Role, "Spooler"),
							new Claim("SpoolerId", spooler.Id.ToString()),
							new Claim("ZoneId", spooler.ZoneId.ToString()),
							new Claim("ProjectId", spooler.ProjectId.ToString())
						}, "Bearer");

					context.User = new ClaimsPrincipal(identity);
				}
			}

			await next(context);
		}

		private static bool TryParseAuthorizationValue(string headerValue, out Guid key)
		{
			var prefix = "Bearer spooler:";
			if (headerValue.StartsWith(prefix) && Guid.TryParse(headerValue.AsSpan(prefix.Length), out key))
			{
				return true;
			}

			key = default;
			return false;
		}
	}
}
