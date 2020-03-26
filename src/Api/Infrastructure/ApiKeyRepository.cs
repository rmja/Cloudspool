using Cloudspool.AspNetCore.Authentication.ApiKey;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Infrastructure
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly CloudspoolContext _db;

        public ApiKeyRepository(CloudspoolContext db)
        {
            _db = db;
        }

        public async Task<ApiKey> GetByKey(string key)
        {
            if (TryParseProjectKey(key, out var projectKey))
            {
                var project = await _db.Projects.SingleOrDefaultAsync(x => x.Key == projectKey);

                if (project is null)
                {
                    return null;
                }

                return new ApiKey(key, new[] {
                    new Claim(ClaimTypes.Role, "Project"),
                    new Claim("ProjectId", project.Id.ToString())
                });
            }
            else if (TryParseSpoolerKey(key, out var spoolerKey))
            {
                var spooler = await _db.Spoolers.Include(x => x.Zone).SingleOrDefaultAsync(x => x.Key == spoolerKey);

                if (spooler is null)
                {
                    return null;
                }

                return new ApiKey(key, new[] {
                    new Claim(ClaimTypes.Role, "Spooler"),
                    new Claim("ProjectId", spooler.Zone.ProjectId.ToString()),
                    new Claim("ZoneId", spooler.ZoneId.ToString())
                });
            }
            else if (TryParseTerminalKey(key, out var terminalKey))
            {
                var terminal = await _db.Terminals.Include(x => x.Zone).SingleOrDefaultAsync(x => x.Key == terminalKey);

                if (terminal is null)
                {
                    return null;
                }

                return new ApiKey(key, new[] {
                    new Claim(ClaimTypes.Role, "Spooler"),
                    new Claim("ProjectId", terminal.Zone.ProjectId.ToString()),
                    new Claim("ZoneId", terminal.ZoneId.ToString())
                });
            }

            return null;
        }

        private static bool TryParseProjectKey(string key, out Guid projectKey)
        {
            var prefix = "project:";
            if (key.StartsWith(prefix) && Guid.TryParse(key.AsSpan(prefix.Length), out projectKey))
            {
                return true;
            }

            projectKey = default;
            return false;
        }

        private static bool TryParseSpoolerKey(string key, out Guid spoolerKey)
        {
            var prefix = "spooler:";
            if (key.StartsWith(prefix) && Guid.TryParse(key.AsSpan(prefix.Length), out spoolerKey))
            {
                return true;
            }

            spoolerKey = default;
            return false;
        }

        private static bool TryParseTerminalKey(string key, out Guid terminalKey)
        {
            var prefix = "terminal:";
            if (key.StartsWith(prefix) && Guid.TryParse(key.AsSpan(prefix.Length), out terminalKey))
            {
                return true;
            }

            terminalKey = default;
            return false;
        }
    }
}
