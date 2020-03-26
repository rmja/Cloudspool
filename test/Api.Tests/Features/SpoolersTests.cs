using Cloudspool.Api.Client.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class SpoolersTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SpoolersTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanCreate()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            var command = new
            {
                Name = "New spooler"
            };

            // When
            var response = await _client.PostAsJsonAsync($"/Zones/{firstZone.Id}/Spoolers", command);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Spooler>();

            // Then
            Assert.Equal(SeedData.TestProject.Id, result.ProjectId);
            Assert.Equal(command.Name, result.Name);
        }

        [Fact]
        public async Task CanDelete()
        {
            // Given
            var firstSpooler = SeedData.GetSpoolers().First();

            // When
            var response = await _client.DeleteAsync($"/Spoolers/{firstSpooler.Id}");
            response.EnsureSuccessStatusCode();

            // Then
        }

        [Fact]
        public async Task CanSetPrinters()
        {
            // Given
            var firstSpooler = SeedData.GetSpoolers().First();
            var command = new[] { "Printer 1", "Printer 2" };

            // When
            var response = await _client.PutAsJsonAsync($"/Spoolers/{firstSpooler.Id}/Printers", command);
            response.EnsureSuccessStatusCode();

            var spoolerResponse = await _client.GetAsync($"/Spoolers/{firstSpooler.Id}");
            var spooler = await spoolerResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Spooler>();

            // Then
            Assert.Equal(2, spooler.Printers.Length);
            Assert.Contains("Printer 1", spooler.Printers);
            Assert.Contains("Printer 2", spooler.Printers);
        }

        [Fact]
        public async Task CanUpdate()
        {
            // Given
            var firstSpooler = SeedData.GetSpoolers().First();
            var patch = new JsonPatchDocument<Spooler>()
                .Replace(x => x.Name, "Updated name");

            // When
            var patchResponse = await _client.PatchAsJsonAsync($"/Spoolers/{firstSpooler.Id}", patch.Operations);
            var getResponse = await _client.FollowRedirectAsync(patchResponse);
            var result = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Spooler>();

            // Then
            Assert.Equal("Updated name", result.Name);
        }

        [Fact]
        public async Task CanGetAll()
        {
            // Given

            // When
            var response = await _client.GetAsync("/Spoolers");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Spooler>>();

            // Then
            Assert.Equal(SeedData.GetSpoolers().Count(), result.Count);
        }

        [Fact]
        public async Task CanGetAllByZoneId()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            firstZone.Spoolers = SeedData.GetSpoolers().Where(x => x.ZoneId == firstZone.Id).ToList();

            // When
            var response = await _client.GetAsync($"/Zones/{firstZone.Id}/Spoolers");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Spooler>>();

            // Then
            Assert.Equal(firstZone.Spoolers.Count, result.Count);
        }

        [Fact]
        public async Task CanGetById()
        {
            // Given
            var firstSpooler = SeedData.GetSpoolers().First();
            firstSpooler.Zone = SeedData.GetZones().First(x => x.Id == firstSpooler.ZoneId);

            // When
            var response = await _client.GetAsync($"/Spoolers/{firstSpooler.Id}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Spooler>();

            // Then
            Assert.Equal(firstSpooler.Id, result.Id);
            Assert.Equal(firstSpooler.Zone.ProjectId, result.ProjectId);
        }

        [Fact]
        public async Task CanGetByKeyAnonymously()
        {
            // Given
            var firstSpooler = SeedData.GetSpoolers().First();
            _client.DefaultRequestHeaders.Clear();

            // When
            var response = await _client.GetAsync($"/Spoolers/{firstSpooler.Key}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Spooler>();

            // Then
            Assert.Equal(firstSpooler.Id, result.Id);
        }
    }
}
