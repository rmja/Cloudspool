using System.Text.RegularExpressions;

namespace Api.Generators
{
    public static class ResourceModuleUtils
    {
        private static readonly Regex _regex = new Regex(@"^resources/([a-zA-Z0-9_-]+\.[a-z]+)$", RegexOptions.Compiled);

        public static string GetResourceAlias(string specifier)
        {
            var match = _regex.Match(specifier);

            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
