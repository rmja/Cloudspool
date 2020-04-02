using System.IO;

namespace Api.Generators.JavaScript.Polyfills
{
    public static class PolyfillScripts
    {
        public static string Get(string name)
        {
            using var stream = typeof(PolyfillScripts).Assembly.GetManifestResourceStream(typeof(PolyfillScripts), $"{name}.js");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
