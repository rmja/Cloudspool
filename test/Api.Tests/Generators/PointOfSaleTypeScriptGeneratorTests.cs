using Api.Generators.ECMAScript6;
using Api.Generators.TypeScript;
using Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Xunit.Abstractions;

namespace Api.Tests.Generators
{
    public partial class PointOfSaleTypeScriptGeneratorTests
    {
        private readonly TypeScriptGenerator _generator;
        private readonly string _kitchenScript;
        private readonly string _receiptScript;

        public PointOfSaleTypeScriptGeneratorTests(ITestOutputHelper output)
        {
            var services = new ServiceCollection()
                .AddSingleton<TypeScriptGenerator>()
                .AddSingleton<ECMAScript6Generator>()
                .AddSingleton<TypeScriptTranspiler>()
                .AddLogging(logging => new XunitLoggerProvider(output))
                .AddMemoryCache()
                .BuildServiceProvider();

            _generator = services.GetRequiredService<TypeScriptGenerator>();

            {
                using var stream = GetType().Assembly.GetManifestResourceStream(GetType(), "kitchen.ts");
                using var reader = new StreamReader(stream);
                _kitchenScript = reader.ReadToEnd();
            }

            {
                using var stream = GetType().Assembly.GetManifestResourceStream(GetType(), "receipt.ts");
                using var reader = new StreamReader(stream);
                _receiptScript = reader.ReadToEnd();
            }
        }
    }

    public class CaseModel
    {
        public int id { get; set; }
        public int packageId { get; set; }
        public int itemId { get; set; }
        public string createdByEmployeeName { get; set; }
        public string note { get; set; }
        public string arrived { get; set; }
        public string departed { get; set; }
    }
}
