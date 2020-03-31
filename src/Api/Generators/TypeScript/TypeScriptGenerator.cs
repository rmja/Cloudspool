using Api.Generators.ECMAScript6;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Api.Generators.TypeScript
{
    public class TypeScriptGenerator : IGenerator
    {
        private readonly TypeScriptTranspiler _tsTranspiler;
        private readonly ECMAScript6Generator _ecmaScript6Generator;
        private readonly IMemoryCache _cache;

        public TypeScriptGenerator(TypeScriptTranspiler tsTranspiler, ECMAScript6Generator ecmaScript6Generator, IMemoryCache cache)
        {
            _tsTranspiler = tsTranspiler;
            _ecmaScript6Generator = ecmaScript6Generator;
            _cache = cache;
        }

        public string[] ValidateTemplate(string code)
        {
            var transpiled = _tsTranspiler.Transpile(code);

            return _ecmaScript6Generator.ValidateTemplate(transpiled);
        }

        public Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null)
        {
            var script = _cache.GetOrCreate(code, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                return _tsTranspiler.Transpile(code);
            });

            return _ecmaScript6Generator.GenerateDocumentAsync(script, model, resourceManager);
        }
    }
}
