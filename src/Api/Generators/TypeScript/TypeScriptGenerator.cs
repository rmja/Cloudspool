using Api.Generators.JavaScript;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Api.Generators.TypeScript
{
    public class TypeScriptGenerator : IGenerator
    {
        private readonly ITypeScriptTranspiler _transpiler;
        private readonly IJavaScriptGenerator _javaScriptGenerator;
        private readonly IMemoryCache _cache;

        public TypeScriptGenerator(ITypeScriptTranspiler transpiler, IJavaScriptGenerator javaScriptGenerator, IMemoryCache cache)
        {
            _transpiler = transpiler;
            _javaScriptGenerator = javaScriptGenerator;
            _cache = cache;
        }

        public string[] ValidateTemplate(string code)
        {
            var transpiled = _transpiler.Transpile(code);

            return _javaScriptGenerator.ValidateTemplate(transpiled);
        }

        public Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null)
        {
            var script = _cache.GetOrCreate(code, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                return _transpiler.Transpile(code);
            });

            return _javaScriptGenerator.GenerateDocumentAsync(script, model, resourceManager);
        }
    }
}
