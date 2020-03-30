using Api.Generators.ECMAScript6;
using Microsoft.Extensions.Caching.Memory;
using NiL.JS;
using NiL.JS.Core;
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
            try
            {
                var (transpiled, diagnostics) = _tsTranspiler.Transpile(code);

                if (diagnostics.Length > 0)
                {
                    return diagnostics;
                }

                return _ecmaScript6Generator.ValidateTemplate(transpiled);
            }
            catch (JSException e)
            {
                return new[] { e.Message };
            }
        }

        public Task<(byte[] Content, string ContentType)> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null)
        {
            var script = _cache.GetOrCreate(code, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                var (transpiled, _) = _tsTranspiler.Transpile(code);
                return Script.Parse(transpiled);
            });

            return _ecmaScript6Generator.GenerateDocumentAsync(script, model, resourceManager);
        }
    }
}
