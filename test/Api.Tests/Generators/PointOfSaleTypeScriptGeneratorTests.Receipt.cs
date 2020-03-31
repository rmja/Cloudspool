using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Generators
{
    public partial class PointOfSaleTypeScriptGeneratorTests
    {
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

            var result = await _generator.GenerateDocumentAsync(_receiptScript, model, resources);

            var expected = File.ReadAllBytes(Path.Combine("Expected", nameof(PointOfSaleTypeScriptGeneratorTests) + "." + nameof(Receipt_New) + ".bin"));

            Assert.Equal(BitConverter.ToString(expected), BitConverter.ToString(result.Content));
        }
    }

    public class ReceiptModel
    {
        public string vendorDataResource { get; set; }
        public int slipId { get; set; }
        public CaseModel @case { get; set; }
        public List<Line> lines { get; set; } = new List<Line>();
        public string itemName { get; set; }
        public string printedByEmployeeName { get; set; }
        public decimal grossTotal { get; set; }
        public decimal discountTotal { get; set; }
        public decimal netTotal { get; set; }
        public decimal netTaxTotal { get; set; }
        public decimal netEuroTotal { get; set; }
        public string printed { get; set; }

        public class Line
        {
            public string name { get; set; }
            public decimal quantity { get; set; }
            public decimal unitPrice { get; set; }
            public decimal discount { get; set; }
        }
    }
}
