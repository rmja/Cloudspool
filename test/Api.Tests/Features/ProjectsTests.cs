using Cloudspool.Api.Client.Models;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class ProjectsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProjectsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanGetByKeyAnonymously()
        {
            // Given
            var firstProject = SeedData.GetProjects().First();
            _client.DefaultRequestHeaders.Clear();

            // When
            var response = await _client.GetAsync($"/Projects/{firstProject.Key}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Project>();

            // Then
            Assert.Equal(firstProject.Key, result.Key);
            Assert.Equal(firstProject.Name, result.Name);
        }
    }
}
