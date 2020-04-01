using Api.Generators.JavaScript;
using Api.Generators.TypeScript;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Api.Generators
{
    public class GeneratorProvider
    {
        private readonly IServiceProvider _services;

        public GeneratorProvider(IServiceProvider services)
        {
            _services = services;
        }

        public IGenerator GetGenerator(string mediaType) => mediaType switch
        {
            "application/javascript" => _services.GetRequiredService<V8JavaScriptGenerator>(),
            "application/typescript" => _services.GetRequiredService<TypeScriptGenerator>(),
            _ => throw new NotSupportedException(),
        };
    }
}
