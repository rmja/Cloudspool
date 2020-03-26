using Microsoft.AspNetCore.Authentication;

namespace Cloudspool.AspNetCore.Authentication.ApiKey
{
    public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string AuthenticationScheme { get; set; } = ApiKeyDefaults.AuthenticationScheme;
        public string AuthenticationType { get; set; } = "API Key Authentication Type";
    }
}
