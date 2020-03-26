using System.Collections.Generic;
using System.Security.Claims;

namespace Cloudspool.AspNetCore.Authentication.ApiKey
{
    public class ApiKey
    {
        public string Key { get; set; }
        public Claim[] Claims { get; }

        public ApiKey(string key, Claim[] claims)
        {
            Key = key;
            Claims = claims;
        }
    }
}
