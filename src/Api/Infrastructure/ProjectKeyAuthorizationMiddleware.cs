using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Infrastructure
{
	public class ProjectKeyAuthorizationMiddleware : IMiddleware
    {
		private readonly CloudspoolContext _db;

		public ProjectKeyAuthorizationMiddleware(CloudspoolContext db)
		{
			_db = db;
		}

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
			if (!context.User.Identity.IsAuthenticated)
			{
				if (context.Request.Headers.TryGetValue(HeaderNames.Authorization, out var headerValue) && TryParseAuthorizationValue(headerValue, out var projectKey))
				{
					var project = await _db.Projects.SingleOrDefaultAsync(x => x.Key == projectKey);
					var identity = new ClaimsIdentity(new[]
						{
							new Claim(ClaimTypes.Role, "Project"),
							new Claim("ProjectId", project.Id.ToString())
						}, "Bearer");

					context.User = new ClaimsPrincipal(identity);
				}
			}

			await next(context);
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
