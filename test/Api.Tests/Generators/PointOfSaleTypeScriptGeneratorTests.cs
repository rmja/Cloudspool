using Api.Generators.ECMAScript6;
using Api.Generators.TypeScript;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Api.Tests.Generators
{
    public partial class PointOfSaleTypeScriptGeneratorTests
    {
        private readonly TypeScriptGenerator _generator;
        private readonly string _kitchenScript;
        private readonly string _receiptScript;

        public PointOfSaleTypeScriptGeneratorTests()
        {
            var services = new ServiceCollection()
                .AddSingleton<TypeScriptGenerator>()
                .AddSingleton<ECMAScript6Generator>()
                .AddSingleton<TypeScriptTranspiler>()
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
        public DateTime arrived { get; set; }
        public DateTime? departed { get; set; }
    }
}
