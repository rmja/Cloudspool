using Api.Generators.ECMAScript6;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Generators
{
    public class ECMAScript6GeneratorTests
    {
        private readonly ECMAScript6Generator _generator;

        public ECMAScript6GeneratorTests()
        {
            var services = new ServiceCollection()
                .AddSingleton<ECMAScript6Generator>()
                .AddMemoryCache()
                .BuildServiceProvider();

            _generator = services.GetRequiredService<ECMAScript6Generator>();
        }

        [Fact]
        public async Task CanBuildString()
        {
            var script = @"
export default class Builder {
    constructor() {
        this.contentType = 'text/plain';
    }
    build(model) {
        return `The result ${model.name}`
    }
}";
            var (result, contentType) = await _generator.GenerateDocumentAsync(script, new { name = "Rasmus" });

            Assert.Equal("The result Rasmus", Encoding.UTF8.GetString(result));
            Assert.Equal("text/plain", contentType);
        }

        [Fact]
        public async Task CanBuildUint8Array()
        {
            var script = @"
export default class Builder {
    constructor() {
        this.contentType = 'text/plain';
    }
    build(model) {
        return new Uint8Array([1,2,3]);
    }
}";

            var (result, _) = await _generator.GenerateDocumentAsync(script, null);

            Assert.Equal(new byte[] { 1, 2, 3 }, result);
        }

        [Fact]
        public async Task CanImportJsonResource()
        {
            var script = @"
import obj from 'resources/alias.json';

export default class Builder {
    constructor() {
        this.contentType = 'text/plain';
    }
    build(model) {
        return obj.name;
    }
}";

            var resources = new DictionaryResourceManager()
            {
                ["alias"] = JsonSerializer.SerializeToUtf8Bytes(new { name = "Rasmus" })
            };

            var (result, _) = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal("Rasmus", Encoding.UTF8.GetString(result));
        }

        [Fact]
        public async Task CanImportBinary()
        {
            var script = @"
import array from 'resources/alias.bin';

export default class Builder {
    constructor() {
        this.contentType = 'text/plain';
    }
    build(model) {
        return [array.length, array[0]];
    }
}";

            var resources = new DictionaryResourceManager()
            {
                ["alias"] = new byte[] { 0xA0 }
            };

            var (result, _) = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal(1, result[0]);
            Assert.Equal(0xA0, result[1]);
        }

        [Fact]
        public async Task CanImportBitmapImageResource()
        {
            var script = @"
import imageData from 'resources/alias.bmp';

export default class Builder {
    constructor() {
        this.contentType = 'text/plain';
    }
    build(model) {
        return [imageData.width, imageData.height, imageData.data.length, imageData.data[0], imageData.data[1], imageData.data[2], imageData.data[3]];
    }
}";

            using var bitmap = new Bitmap(2, 2);
            using var stream = new MemoryStream();
            bitmap.SetPixel(0, 0, Color.FromArgb(red: 1, green: 2, blue: 3, alpha: 255));
            bitmap.Save(stream, ImageFormat.Bmp);

            var resources = new DictionaryResourceManager()
            {
                ["alias"] = stream.ToArray()
            };

            var (result, _) = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal(2, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(16, result[2]);
            Assert.Equal(1, result[3]);
            Assert.Equal(2, result[4]);
            Assert.Equal(3, result[5]);
            Assert.Equal(255, result[6]);
        }

        [Fact(Skip = "Not yet supported in Nil.JS, see https://github.com/nilproject/NiL.JS/issues/213")]
        public async Task CanDynamicImportJsonResource()
        {
            var script = @"
export default class Builder {
    async build(model) {
        let obj = await get('resources/alias.jsob');
        return obj;
    }
}";

            var resources = new DictionaryResourceManager()
            {
                ["alias"] = JsonSerializer.SerializeToUtf8Bytes(new { name = "Rasmus" })
            };

            var (result, _) = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal("Rasmus", Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ReturnsErrorOnInvalidTemplate()
        {
            var errors = _generator.ValidateTemplate("varr a = 2");

            Assert.NotEmpty(errors);
        }
    }
}
