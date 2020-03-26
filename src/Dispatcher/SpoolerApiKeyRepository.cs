using Cloudspool.Api.Client;
using Cloudspool.AspNetCore.Authentication.ApiKey;
using Refit;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dispatcher
{
	public class SpoolerApiKeyRepository : IApiKeyRepository
    {
		private readonly IApiClient _api;

		public SpoolerApiKeyRepository(IApiClient api)
		{
			_api = api;
		}

		public async Task<ApiKey> GetByKey(string key)
		{
			if (!TryParseKey(key, out var spoolerKey))
			{
				return null;
			}

			try
			{
				var spooler = await _api.SpoolerGetByKeyAsync(spoolerKey);

				return new ApiKey(key, new[]
				{
					new Claim(ClaimTypes.Role, "Spooler"),
					new Claim("SpoolerId", spooler.Id.ToString()),
					new Claim("ZoneId", spooler.ZoneId.ToString()),
					new Claim("ProjectId", spooler.ProjectId.ToString())
				});
			}
			catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
			{
				return null;
			}
		}

		private static bool TryParseKey(string key, out Guid spoolerKey)
		{
			var prefix = "spooler:";
			if (key.StartsWith(prefix) && Guid.TryParse(key.AsSpan(prefix.Length), out spoolerKey))
			{
				return true;
			}

			spoolerKey = default;
			return false;
		}
	}
}
