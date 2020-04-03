using Api.Generators.JavaScript;
using ChakraCore.API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Generators
{
    public class JavaScriptGeneratorTests_ChakraCore : JavaScriptGeneratorTests<ChakraCoreJavaScriptGenerator>
    {
    }

    public class JavaScriptGeneratorTests_V8 : JavaScriptGeneratorTests<V8JavaScriptGenerator>
    {
    }

    public abstract class JavaScriptGeneratorTests<TJavaScriptGenerator>
        where TJavaScriptGenerator : class, IJavaScriptGenerator
    {
        private readonly IJavaScriptGenerator _generator;
        private readonly ResourceScriptFactory _resourceScriptFactory;

        public JavaScriptGeneratorTests()
        {
            var services = new ServiceCollection()
                .AddSingleton<IJavaScriptGenerator, TJavaScriptGenerator>()
                .AddSingleton<ResourceScriptFactory>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddMemoryCache()
                .BuildServiceProvider();

            _generator = services.GetRequiredService<IJavaScriptGenerator>();
            _resourceScriptFactory = services.GetRequiredService<ResourceScriptFactory>();
        }

        [Fact]
        public async Task CanBuildString()
        {
            var script = @"
export default class Builder {
    build(model) {
        return `The result ${model.name}`;
    }
}";
            
            var result = await _generator.GenerateDocumentAsync(script, new { name = "Rasmus" });

            Assert.Equal("The result Rasmus", Encoding.UTF8.GetString(result.Content));
            Assert.Equal("text/plain", result.ContentType);
        }

        [Fact]
        public async Task CanBuildPromiseString()
        {
            var script = @"
export default class Builder {
    build(model) {
        return Promise.resolve(`The result ${model.name}`);
    }
}";

            var result = await _generator.GenerateDocumentAsync(script, new { name = "Rasmus" });

            Assert.Equal("The result Rasmus", Encoding.UTF8.GetString(result.Content));
            Assert.Equal("text/plain", result.ContentType);
        }

        [Fact]
        public async Task CanBuildUint8Array()
        {
            var script = @"
export default class Builder {
    build(model) {
        const buffer = new ArrayBuffer(16);
        const array = new Uint8Array(buffer, 4, 3);
        array[0] = 1;
        array[1] = 2;
        array[2] = 3;
        return array;
    }
}";

            var result = await _generator.GenerateDocumentAsync(script, null);

            Assert.Equal(new byte[] { 1, 2, 3 }, result.Content);
            Assert.Equal("application/octet-stream", result.ContentType);
        }

        [Fact]
        public async Task CanOverrideContentType()
        {
            var script = @"
export const contentType = 'test/test';
export default class Builder {
    build(model) {
        return `The result ${model.name}`
    }
}";

            var result = await _generator.GenerateDocumentAsync(script, new { name = "Rasmus" });

            Assert.Equal("The result Rasmus", Encoding.UTF8.GetString(result.Content));
            Assert.Equal("test/test", result.ContentType);
        }

        [Fact]
        public async Task CanImportJsonResource()
        {
            var script = @"
import obj from 'resources/alias.json';

export default class Builder {
    build(model) {
        return obj.name;
    }
}";

            var resources = new DictionaryResourceManager()
            {
                ["alias.json"] = JsonSerializer.SerializeToUtf8Bytes(new { name = "Rasmus" })
            };

            var result = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal("Rasmus", Encoding.UTF8.GetString(result.Content));
        }

        [Fact]
        public async Task CanImportBinaryResource()
        {
            var script = @"
import array from 'resources/alias.bin';

export default class Builder {
    build(model) {
        return [array.length, array[0]];
    }
}";

            var resources = new DictionaryResourceManager()
            {
                ["alias.bin"] = new byte[] { 0xA0 }
            };

            var result = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal(1, result.Content[0]);
            Assert.Equal(0xA0, result.Content[1]);
        }

        [Fact]
        public async Task CanImportSmallBitmapImageResource()
        {
            var script = @"
import imageData from 'resources/alias.bmp';

export default class Builder {
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
                ["alias.bmp"] = stream.ToArray()
            };

            var result = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal(2, result.Content[0]);
            Assert.Equal(2, result.Content[1]);
            Assert.Equal(16, result.Content[2]);
            Assert.Equal(1, result.Content[3]);
            Assert.Equal(2, result.Content[4]);
            Assert.Equal(3, result.Content[5]);
            Assert.Equal(255, result.Content[6]);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        public async Task CanImportLargeBitmapImageResource(int asyncLoadDelay)
        {
            var script = @"
import imageData from 'resources/havnebakken.bmp';

export default class Builder {
    build(model) {
        return JSON.stringify([imageData.width, imageData.height, imageData.data.length]);
    }
}";

            var resources = new DictionaryResourceManager()
            {
                ["havnebakken.bmp"] = File.ReadAllBytes("havnebakken.bmp")
            };
            resources.AsyncLoadDelay = TimeSpan.FromMilliseconds(asyncLoadDelay);

            var result = await _generator.GenerateDocumentAsync(script, null, resources);

            var array = JsonSerializer.Deserialize<int[]>(Encoding.UTF8.GetString(result.Content));
            Assert.Equal(350, array[0]);
            Assert.Equal(260, array[1]);
            Assert.Equal(350 * 260 * 4, array[2]);
        }

        [Fact]
        public async Task CanDynamicImportJsonResource()
        {
            var script = @"
export default class Builder {
    async build(model) {
        const { default: obj } = await import('resources/alias.json');
        return obj.name;
    }
}";

            var resources = new DictionaryResourceManager()
            {
                ["alias.json"] = JsonSerializer.SerializeToUtf8Bytes(new { name = "Rasmus" })
            };

            var result = await _generator.GenerateDocumentAsync(script, null, resources);

            Assert.Equal("Rasmus", Encoding.UTF8.GetString(result.Content));
        }

        [Fact]
        public async Task ThrowsIfDynamicImportOfInvalidResource()
        {
            var script = @"
export default class Builder {
    async build(model) {
        const { default: obj } = await import('resources/invalid.json');
        return obj.name;
    }
}";

            var resources = new DictionaryResourceManager()
            {
            };

            await Assert.ThrowsAsync<JavaScriptScriptException>(async () =>
            {
                var result = await _generator.GenerateDocumentAsync(script, null, resources);

                Assert.Equal("Rasmus", Encoding.UTF8.GetString(result.Content));
            });
        }

        [Fact]
        public void CanValidateLargeModule()
        {
            var bitmap = File.ReadAllBytes("havnebakken.bmp");
            var script = _resourceScriptFactory.CreateFromExtension(bitmap, ".bmp");

            var result = _generator.ValidateTemplate(script);

            Assert.Empty(result);
        }

        [Fact]
        public void ReturnsErrorOnInvalidTemplate()
        {
            var errors = _generator.ValidateTemplate("varr a = 2");

            Assert.NotEmpty(errors);
        }
    }
}
