using Api.Generators.JavaScript;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Api.Generators.TypeScript
{
    public class TypeScriptGenerator : IGenerator
    {
        private readonly V8TypeScriptTranspiler _tsTranspiler;
        private readonly V8JavaScriptGenerator _javaScriptGenerator;
        private readonly IMemoryCache _cache;

        public TypeScriptGenerator(V8TypeScriptTranspiler tsTranspiler, V8JavaScriptGenerator javaScriptGenerator, IMemoryCache cache)
        {
            _tsTranspiler = tsTranspiler;
            _javaScriptGenerator = javaScriptGenerator;
            _cache = cache;
        }

        public string[] ValidateTemplate(string code)
        {
            var transpiled = _tsTranspiler.Transpile(code);

            return _javaScriptGenerator.ValidateTemplate(transpiled);
        }

        public Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null)
        {
            var script = _cache.GetOrCreate(code, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                return _tsTranspiler.Transpile(code);
            });

            return _javaScriptGenerator.GenerateDocumentAsync(script, model, resourceManager);
        }
    }
}
