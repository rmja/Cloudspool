using Cloudspool.Api.Client.Models;
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

            // When
            var putResponse = await _client.PutAsync("/Resources/the-new-alias/Content", resourceContent);
            var getResponse = await _client.FollowRedirectAsync(putResponse);
            var resource = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Resource>();

            var contentResponse = await _client.GetAsync("/Resources/the-new-alias/Content");
            var content = await contentResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<string>();

            var resourcesResponse = await _client.GetAsync("/Resources");
            var resources = await resourcesResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Resource>>();

            // Then
            Assert.Equal("the-new-alias", resource.Alias);
            Assert.Equal("application/json", contentResponse.Content.Headers.ContentType.ToString());
            Assert.Equal("the new resource", content);
            Assert.Equal(SeedData.GetResources().Count() + 1, resources.Count);
            Assert.Contains("the-new-alias", resources.Select(x => x.Alias));
        }

        [Fact]
        public async Task CanSet_Update()
        {
            // Given
            var firstResource = SeedData.GetResources().First();
            var resourceContent = new StringContent("\"the updated resource\"", Encoding.UTF8, "application/json");
            
            // When
            var putResponse = await _client.PutAsync($"/Resources/{firstResource.Alias}/Content", resourceContent);
            var getResponse = await _client.FollowRedirectAsync(putResponse);
            var resource = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Resource>();

            var contentResponse = await _client.GetAsync($"/Resources/{firstResource.Alias}/Content");
            var content = await contentResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<string>();

            var resourcesResponse = await _client.GetAsync("/Resources");
            var resources = await resourcesResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Resource>>();

            // Then
            Assert.Equal(firstResource.Alias, resource.Alias);
            Assert.Equal("application/json", contentResponse.Content.Headers.ContentType.ToString());
            Assert.Equal("the updated resource", content);
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
            Assert.Equal($"/Resources/{firstResource.Alias}/Content", result.ContentUrl);
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
            Assert.Equal($"{firstResource.Alias}.json", response.Content.Headers.ContentDisposition.FileName);
            Assert.Equal(firstResource.MediaType, response.Content.Headers.ContentType.MediaType);
            Assert.Equal("a string", result);
        }
    }
}
