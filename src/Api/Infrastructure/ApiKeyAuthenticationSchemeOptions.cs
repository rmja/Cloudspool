using Microsoft.AspNetCore.Authentication;

namespace Api.Infrastructure
{
    public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string AuthenticationScheme { get; set; } = ApiKeyDefaults.AuthenticationScheme;
        public string AuthenticationType { get; set; } = "API Key Authentication Type";
    }
}
