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
    public class ReceiptStarLinePointOfSaleTypeScriptGeneratorTests_ChakraCore : ReceiptStarLinePointOfSaleTypeScriptGeneratorTests<ChakraCoreJavaScriptGenerator, ChakraCoreTypeScriptTranspiler>
    {
    }

    public class ReceiptStarLinePointOfSaleTypeScriptGeneratorTests_V8 : ReceiptStarLinePointOfSaleTypeScriptGeneratorTests<V8JavaScriptGenerator, V8TypeScriptTranspiler>
    {
    }

    public abstract class ReceiptStarLinePointOfSaleTypeScriptGeneratorTests<TJavaScriptGenerator, TTypeScriptTranspiler> : PointOfSaleTypeScriptGeneratorTestBase<TJavaScriptGenerator, TTypeScriptTranspiler>
        where TJavaScriptGenerator : class, IJavaScriptGenerator
        where TTypeScriptTranspiler : class, ITypeScriptTranspiler
    {
        public ReceiptStarLinePointOfSaleTypeScriptGeneratorTests() : base("receipt-starline.ts")
        {
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        public async Task Receipt_New(int asyncLoadDelay)
        {
            var model = new ReceiptModel()
            {
                vendorDataResource = "havnebakken.json",
                slipId = 317579,
                @case = new CaseModel()
                {
                    id = 1538006,
                    packageId = 1,
                    itemId = 1,
                    createdByEmployeeName = "Malene",
                    note = null,
                    arrived = new DateTime(2020, 3, 20, 19, 31, 0).ToString("o"),
                    departed = null,
                    
                },
                lines =
                {
                    new ReceiptModel.Line() { name = "Fish and chips UAH", quantity = 1, unitPrice = 55, discount = 0 }
                },
                itemName = " 6",
                printedByEmployeeName = "Malene",
                grossTotal = 55,
                discountTotal = 0,
                netTotal = 55,
                netTaxTotal = 11,
                netEuroTotal = 7.53m,                
                printed = new DateTime(2020, 3, 20, 19, 31, 0).ToString("o"),
            };

            var resources = new DictionaryResourceManager()
            {
                ["havnebakken.json"] = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    logoResource = "havnebakken.bmp",
                    address = "Havnebakken 12",
                    postalCode = 9940,
                    city = "Læsø",
                    phone = 98499009,
                    cvr = 20041846,
                    email = "post@havnebakken.dk",
                    logoOffsetLeft = 100
                }),
                ["havnebakken.bmp"] = File.ReadAllBytes("havnebakken.bmp")
            };
            resources.AsyncLoadDelay = TimeSpan.FromMilliseconds(asyncLoadDelay);

            var result = await _generator.GenerateDocumentAsync(_script, model, resources);

            var expected = File.ReadAllBytes(Path.Combine("Expected", "ReceiptStarLinePointOfSaleTypeScriptGeneratorTests.Receipt_New.bin"));

            Assert.Equal(BitConverter.ToString(expected), BitConverter.ToString(result.Content));
        }
    }
}
