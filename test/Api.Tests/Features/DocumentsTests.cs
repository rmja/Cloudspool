using Cloudspool.Api.Client.Models;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class DocumentsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public DocumentsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanCreate()
        {
            // Given
            var content = new StringContent("Test Document", Encoding.UTF8, "text/plain");

            // When
            var response = await _client.PostAsync("/Documents", content);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Document>();

            var contentResponse = await _client.GetAsync(result.ContentUrl);
            var contentString = await contentResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // Then
            Assert.Equal(SeedData.TestProject.Id, result.ProjectId);
            Assert.Equal("text/plain; charset=utf-8", result.ContentType);
            Assert.Equal("Test Document", contentString);
        }

        [Fact]
        public async Task CanDelete()
        {
            // Given
            var firstDocument = SeedData.GetDocuments().First();

            // When
            var response = await _client.DeleteAsync($"/Documents/{firstDocument.Id}");
            response.EnsureSuccessStatusCode();

            // Then
        }

        [Fact]
        public async Task CanGenerateAsProject()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            var model = new
            {
                name = "Rasmus"
            };

            // When
            var response = await _client.PostAsJsonAsync($"/Zones/{firstZone.Id}/Documents/Generate?format=slip", model);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Document>();

            var contentResponse = await _client.GetAsync(result.ContentUrl);
            var contentString = await contentResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // Then
            Assert.Equal("text/plain", result.ContentType);
            Assert.Equal("name: Rasmus, json: {\"name\":\"Rasmus\"}", contentString);
        }

        [Fact]
        public async Task CanGenerateAsTerminal()
        {
            // Given
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"terminal:{SeedData.TestTerminalKey}");
            var model = new
            {
                name = "Rasmus"
            };

            // When
            var response = await _client.PostAsJsonAsync($"/Documents/Generate?format=slip", model);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Document>();

            var contentResponse = await _client.GetAsync(result.ContentUrl);
            var contentString = await contentResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // Then
            Assert.Equal("text/plain", result.ContentType);
            Assert.Equal("name: Rasmus, json: {\"name\":\"Rasmus\"}", contentString);
        }

        [Fact]
        public async Task CanGetAll()
        {
            // Given

            // When
            var response = await _client.GetAsync("/Documents");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Document>>();

            // Then
            Assert.Equal(SeedData.GetDocuments().Count(), result.Count);
        }

        [Fact]
        public async Task CanGetById()
        {
            // Given
            var firstDocument = SeedData.GetDocuments().First();

            // When
            var response = await _client.GetAsync($"/Documents/{firstDocument.Id}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Document>();

            // Then
            Assert.Equal(firstDocument.Id, result.Id);
            Assert.Equal(firstDocument.ProjectId, result.ProjectId);
        }

        [Fact]
        public async Task CanGetContentById()
        {
            // Given
            var firstDocument = SeedData.GetDocuments().First();

            // When
            var response = await _client.GetAsync($"/Documents/{firstDocument.Id}/Content");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // Then
            Assert.Equal(firstDocument.ContentType, response.Content.Headers.ContentType.MediaType);
            Assert.Equal(Encoding.UTF8.GetString(firstDocument.Content), result);
        }
    }
}
