using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Generators
{
    public class ReceiptStarLinePointOfSaleTypeScriptGeneratorTests : PointOfSaleTypeScriptGeneratorTestBase
    {
        public ReceiptStarLinePointOfSaleTypeScriptGeneratorTests() : base("receipt-starline.ts")
        {

        }

        [Fact]
        public async Task Receipt_New()
        {
            var model = new ReceiptModel()
            {
                vendorDataResource = "pos_slip_receipt_havnebakken_data",
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
                ["pos_slip_receipt_havnebakken_data.json"] = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    logoResource = "pos_slip_havnebakken_logo_2",
                    address = "Havnebakken 12",
                    postalCode = 9940,
                    city = "Læsø",
                    phone = 98499009,
                    cvr = 20041846,
                    email = "post@havnebakken.dk",
                    logoOffsetLeft = 100
                }),
                ["pos_slip_havnebakken_logo_2.bmp"] = File.ReadAllBytes("havnebakken.bmp")
            };

            var result = await _generator.GenerateDocumentAsync(_script, model, resources);

            var expected = File.ReadAllBytes(Path.Combine("Expected", nameof(ReceiptStarLinePointOfSaleTypeScriptGeneratorTests) + "." + nameof(Receipt_New) + ".bin"));

            Assert.Equal(BitConverter.ToString(expected), BitConverter.ToString(result.Content));
        }
    }
}
