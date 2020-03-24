using Api.Client.Models;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class FormatsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public FormatsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanDelete()
        {
            // Given
            var firstFormat = SeedData.GetFormats().First();

            // When
            var response = await _client.DeleteAsync($"/Zones/{firstFormat.ZoneId}/Formats/{firstFormat.Alias}");
            response.EnsureSuccessStatusCode();

            // Then
        }

        [Fact]
        public async Task CanSet()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            var firstTemplate = SeedData.GetTemplates().First();
            var command = new Format()
            {
                TemplateId = firstTemplate.Id
            };

            // When
            var putResponse = await _client.PutAsJsonAsync($"/Zones/{firstZone.Id}/Formats/TheAlias", command);
            var getResponse = await _client.FollowRedirectAsync(putResponse);
            var result = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Format>();

            // Then
            Assert.Equal(firstZone.Id, result.ZoneId);
            Assert.Equal("TheAlias", result.Alias);
            Assert.Equal(command.TemplateId, result.TemplateId);
        }

        [Fact]
        public async Task CanGetAllByZone()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            firstZone.Formats = SeedData.GetFormats().Where(x => x.ZoneId == firstZone.Id).ToList();

            // When
            var response = await _client.GetAsync($"/Zones/{firstZone.Id}/Formats");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Format>>();

            // Then
            Assert.Equal(firstZone.Formats.Count(), result.Count);
        }
    }
}
