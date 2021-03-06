﻿using Api.Generators.JavaScript;
using Api.Generators.JavaScript.ChakraCore;
using Api.Generators.JavaScript.V8;
using Api.Generators.TypeScript;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Generators
{
    public class TypeScriptGeneratorTests_ChakraCore : TypeScriptGeneratorTests<ChakraCoreTypeScriptTranspiler, ChakraCoreJavaScriptGenerator>
    {
    }

    public class TypeScriptGeneratorTests_V8 : TypeScriptGeneratorTests<V8TypeScriptTranspiler, V8JavaScriptGenerator>
    {
    }

    public abstract class TypeScriptGeneratorTests<TTypeScriptTranspiler, TJavaScriptGenerator>
        where TTypeScriptTranspiler: class, ITypeScriptTranspiler
        where TJavaScriptGenerator: class, IJavaScriptGenerator
    {
        private readonly TypeScriptGenerator _generator;

        public TypeScriptGeneratorTests()
        {
            var services = new ServiceCollection()
                .AddSingleton<TypeScriptGenerator>()
                .AddSingleton<ITypeScriptTranspiler, TTypeScriptTranspiler>()
                .AddSingleton<IJavaScriptGenerator, TJavaScriptGenerator>()
                .AddSingleton<ResourceScriptFactory>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddMemoryCache()
                .BuildServiceProvider();

            _generator = services.GetRequiredService<TypeScriptGenerator>();
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
        public async Task CanBuildUint8Array()
        {
            var script = @"
export default class Builder {
    build(model) {
        return new Uint8Array([1,2,3]);
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
        public async Task CanImportBitmapImageResource()
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

        [Fact]
        public async Task CanDynamicImportJsonResource()
        {
            var script = @"
export default class Builder {
    async build(model) {
        let { default: obj } = await import('resources/alias.json');
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
        public void ReturnsErrorOnInvalidTemplate()
        {
            var errors = _generator.ValidateTemplate("varr a = 2");

            Assert.NotEmpty(errors);
        }
    }
}
