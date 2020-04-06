using Api.Generators.JavaScript;
using Api.Generators.JavaScript.ChakraCore;
using Api.Generators.JavaScript.V8;
using Api.Generators.TypeScript;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Generators
{
    public class ReceiptEpsonPointOfSaleTypeScriptGeneratorTests_ChakraCore : ReceiptEpsonPointOfSaleTypeScriptGeneratorTests<ChakraCoreJavaScriptGenerator, ChakraCoreTypeScriptTranspiler>
    {
    }

    public class ReceiptEpsonPointOfSaleTypeScriptGeneratorTests_V8 : ReceiptEpsonPointOfSaleTypeScriptGeneratorTests<V8JavaScriptGenerator, V8TypeScriptTranspiler>
    {
    }

    public abstract class ReceiptEpsonPointOfSaleTypeScriptGeneratorTests<TJavaScriptGenerator, TTypeScriptTranspiler> : PointOfSaleTypeScriptGeneratorTestBase<TJavaScriptGenerator, TTypeScriptTranspiler>
        where TJavaScriptGenerator : class, IJavaScriptGenerator
        where TTypeScriptTranspiler : class, ITypeScriptTranspiler
    {
        public ReceiptEpsonPointOfSaleTypeScriptGeneratorTests() : base("receipt-epson.ts")
        {
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        public async Task Receipt_New(int asyncLoadDelay)
        {
            var model = new ReceiptModel()
            {
                vendorDataResource = "laesoecamping.json",
                slipId = 60558,
                @case = new CaseModel()
                {
                    id = 1028947,
                    packageId = 1,
                    itemId = 1,
                    createdByEmployeeName = "Karina",
                    note = null,
                    arrived = new DateTime(2016, 5, 8, 8, 27, 0).ToString("o"),
                    departed = null,
                    
                },
                lines =
                {
                    new ReceiptModel.Line() { name = "Varesalg Butik", quantity = 1, unitPrice = 10, discount = 0 },
                    new ReceiptModel.Line() { name = "0,5 Sodavand", quantity = 1, unitPrice = 20, discount = 0 },
                    new ReceiptModel.Line() { name = "0,5 Sodavand", quantity = 1, unitPrice = 20, discount = 0 },
                    new ReceiptModel.Line() { name = "Varesalg Butik", quantity = 1, unitPrice = 45, discount = 0 }
                },
                itemName = "Kasse",
                printedByEmployeeName = "Karina",
                grossTotal = 95,
                discountTotal = 0,
                netTotal = 95,
                netTaxTotal = 19,
                netEuroTotal = 12.84m,                
                printed = new DateTime(2016, 5, 8, 10, 14, 0).ToString("o"),
            };

            var resources = new DictionaryResourceManager()
            {
                ["laesoecamping.json"] = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    logoResource = "laesoecamping.bmp",
                    address = "Agersigen 18a",
                    postalCode = 9940,
                    city = "Læsø",
                    phone = 98499495,
                    cvr = 31840902,
                    email = "ferie@laesoecamping.dk",
                    logoOffsetLeft = 100
                }),
                ["laesoecamping.bmp"] = File.ReadAllBytes("laesoecamping.bmp")
            };
            resources.AsyncLoadDelay = TimeSpan.FromMilliseconds(asyncLoadDelay);

            var result = await _generator.GenerateDocumentAsync(_script, model, resources);

            var expected = File.ReadAllBytes(Path.Combine("Expected", "ReceiptEpsonPointOfSaleTypeScriptGeneratorTests.Receipt_New.bin"));

            Assert.Equal(BitConverter.ToString(expected), BitConverter.ToString(result.Content));
        }
    }
}
