using Api.Client.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class ZonesTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ZonesTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanCreate()
        {
            // Given
            var firstSpooler = SeedData.GetSpoolers().First();
            var command = new
            {
                name = "Test Zone",
                routes = new[]
                {
                    new Zone.Route() { Alias = "ZoneRoute", SpoolerId = firstSpooler.Id, PrinterName = "Test Printer" }
                }
            };

            // When
            var response = await _client.PostAsJsonAsync("/Zones", command);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Zone>();

            // Then
            Assert.Equal(SeedData.TestProject.Id, result.ProjectId);
            Assert.Equal(command.name, result.Name);
            var route = Assert.Single(result.Routes);
            Assert.Equal(command.routes[0].Alias, route.Alias);
            Assert.Equal(command.routes[0].SpoolerId, route.SpoolerId);
            Assert.Equal(command.routes[0].PrinterName, route.PrinterName);
        }

        [Fact]
        public async Task CanDelete()
        {
            // Given
            var firstZone = SeedData.GetZones().First();

            // When
            var response = await _client.DeleteAsync($"/Zones/{firstZone.Id}");
            response.EnsureSuccessStatusCode();

            // Then
        }

        [Fact]
        public async Task CanUpdate()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            var firstSpooler = SeedData.GetSpoolers().First();
            var patch = new JsonPatchDocument<Zone>()
                .Replace(x => x.Name, "Updated Name")
                .Add(x => x.Routes, new Zone.Route() { Alias = "UpdatedAlias", SpoolerId = firstSpooler.Id, PrinterName = "Updated Printer" });

            // When
            var response = await _client.PatchAsJsonAsync($"/Zones/{firstZone.Id}", patch.Operations);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Zone>();

            // Then
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(firstZone.Routes.Count + 1, result.Routes.Count);
            var addedRoute = result.Routes.Find(x => x.Alias == "UpdatedAlias");
            Assert.Equal(firstSpooler.Id, addedRoute.SpoolerId);
            Assert.Equal("Updated Printer", addedRoute.PrinterName);
        }

        [Fact]
        public async Task CanGetAll()
        {
            // Given

            // When
            var response = await _client.GetAsync("/Zones");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Zone>>();

            // Then
            Assert.Equal(SeedData.GetZones().Count(), result.Count);
        }

        [Fact]
        public async Task CanGetById()
        {
            // Given
            var firstZone = SeedData.GetZones().First();

            // When
            var response = await _client.GetAsync($"/Zones/{firstZone.Id}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Zone>();

            // Then
            Assert.Equal(firstZone.Id, result.Id);
            Assert.Equal(firstZone.ProjectId, result.ProjectId);
            Assert.Equal(firstZone.Routes.Count, result.Routes.Count);
        }
    }
}
