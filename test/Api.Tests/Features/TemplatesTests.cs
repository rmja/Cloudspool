using Api.Client.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class TemplatesTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TemplatesTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanCreate()
        {
            // Given
            var script = @"
export default class Builder {
    constructor() {
        this.contentType = 'application/escpos';
    }
    build(model) {
    }
}";
            var scriptContent = new StringContent(script, Encoding.UTF8, "application/javascript");
            var scriptContentType = scriptContent.Headers.ContentType.ToString();

            // When
            var response = await _client.PostAsync("/Templates?name=Test Template", scriptContent);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Template>();

            // When
            var scriptResponse = await _client.GetAsync(result.ScriptUrl);
            var scriptResult = await scriptResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // Then
            Assert.Equal("Test Template", result.Name);
            Assert.Equal(script, scriptResult);
            Assert.Equal(scriptContentType, scriptResponse.Content.Headers.ContentType.ToString());
            Assert.Equal(scriptContentType, result.ScriptContentType);
        }

        [Fact]
        public async Task CanUpdate()
        {
            // Given
            var firstTemplate = SeedData.GetTemplates().First();
            var patch = new JsonPatchDocument<Template>()
                .Replace(x => x.Name, "Updated name");

            // When
            var response = await _client.PatchAsJsonAsync($"/Templates/{firstTemplate.Id}", patch.Operations);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Template>();

            // Then
            Assert.Equal("Updated name", result.Name);
        }

        [Fact]
        public async Task CanGetById()
        {
            // Given
            var firstTemplate = SeedData.GetTemplates().First();

            // When
            var response = await _client.GetAsync($"/Templates/{firstTemplate.Id}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Template>();

            // Then
            Assert.Equal(firstTemplate.Id, result.Id);
            Assert.Equal(firstTemplate.ScriptContentType, result.ScriptContentType);
        }

        [Fact]
        public async Task CanGetScriptById()
        {
            // Given
            var firstTemplate = SeedData.GetTemplates().First();

            // When
            var response = await _client.GetAsync($"/Templates/{firstTemplate.Id}/Script");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // Then
            Assert.Equal(firstTemplate.ScriptContentType, response.Content.Headers.ContentType.MediaType);
            Assert.Equal(firstTemplate.Script, result);
        }
    }
}
