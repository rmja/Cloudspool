using Api.Client.Models;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class ResourcesTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ResourcesTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanDelete()
        {
            // Given
            var firstResource = SeedData.GetResources().First();

            // When
            var response = await _client.DeleteAsync($"/Resources/{firstResource.Alias}");
            response.EnsureSuccessStatusCode();

            // Then
        }

        [Fact]
        public async Task CanSet_Create()
        {
            // Given
            var resourceContent = new StringContent("\"the new resource\"", Encoding.UTF8, "application/json");
            var resourceContentType = resourceContent.Headers.ContentType.ToString();

            // When
            var putResponse = await _client.PutAsync("/Resources/the-new-alias", resourceContent);
            putResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/Resources/the-new-alias");
            var resource = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Resource>();

            var resourcesResponse = await _client.GetAsync("/Resources");
            var resources = await resourcesResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Resource>>();

            // Then
            Assert.Equal(resourceContentType, resource.ContentType);
            Assert.Equal("/Resources/the-new-alias/Content", resource.ContentUrl);
            Assert.Equal(SeedData.GetResources().Count() + 1, resources.Count);
            Assert.Contains("the-new-alias", resources.Select(x => x.Alias));
        }

        [Fact]
        public async Task CanSet_Update()
        {
            // Given
            var firstResource = SeedData.GetResources().First();
            var resourceContent = new StringContent("\"the updated resource\"", Encoding.UTF8, "application/json");
            var resourceContentType = resourceContent.Headers.ContentType.ToString();
            
            // When
            var response = await _client.PutAsync($"/Resources/{firstResource.Alias}", resourceContent);
            response.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/Resources/{firstResource.Alias}");
            var resource = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Resource>();

            var resourcesResponse = await _client.GetAsync("/Resources");
            var resources = await resourcesResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Resource>>();

            // Then
            Assert.Equal(resourceContentType, resource.ContentType);
            Assert.Equal($"/Resources/{firstResource.Alias}/Content", resource.ContentUrl);
            Assert.Equal(SeedData.GetResources().Count(), resources.Count);
            Assert.Contains(firstResource.Alias, resources.Select(x => x.Alias));
        }

        [Fact]
        public async Task CanGetAll()
        {
            // Given

            // When
            var response = await _client.GetAsync("/Resources");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Resource>>();

            // Then
            Assert.Equal(SeedData.GetResources().Count(), result.Count);
        }

        [Fact]
        public async Task CanGetByAlias()
        {
            // Given
            var firstResource = SeedData.GetResources().First();

            // When
            var response = await _client.GetAsync($"/Resources/{firstResource.Alias}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Resource>();

            // Then
            Assert.Equal(firstResource.Id, result.Id);
            Assert.Equal($"/Resources/{firstResource.Alias}/Content", result.ContentUrl);
            Assert.Equal(firstResource.ContentType, result.ContentType);
        }

        [Fact]
        public async Task CanGetContentByAlias()
        {
            // Given
            var firstResource = SeedData.GetResources().First();

            // When
            var response = await _client.GetAsync($"/Resources/{firstResource.Alias}/Content");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<string>();

            // Then
            Assert.Equal(firstResource.ContentType, response.Content.Headers.ContentType.MediaType);
            Assert.Equal("a string", result);
        }
    }
}
