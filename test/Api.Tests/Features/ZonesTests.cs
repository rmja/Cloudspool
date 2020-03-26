using Cloudspool.Api.Client.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
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
            var command = new Zone()
            {
                Name = "Test Zone",
                Routes =
                {
                    ["ZoneRoute"] = new Zone.Route() { SpoolerId = firstSpooler.Id, PrinterName = "Test Printer" }
                }
            };

            // When
            var response = await _client.PostAsJsonAsync("/Zones", command);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Zone>();

            // Then
            Assert.Equal(SeedData.TestProject.Id, result.ProjectId);
            Assert.Equal(command.Name, result.Name);
            var route = Assert.Single(result.Routes);
            Assert.Equal(command.Routes.First().Key, route.Key);
            Assert.Equal(command.Routes.First().Value.SpoolerId, route.Value.SpoolerId);
            Assert.Equal(command.Routes.First().Value.PrinterName, route.Value.PrinterName);
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
                .Replace(x => x.Name, "Updated Name");
            patch.Operations.Add(new Operation<Zone>("add", "/routes/UpdatedAlias", null, new Zone.Route() { Alias = "UpdatedAlias", SpoolerId = firstSpooler.Id, PrinterName = "Updated Printer" }));

            // When
            var patchResponse = await _client.PatchAsJsonAsync($"/Zones/{firstZone.Id}", patch.Operations);
            var getResponse = await _client.FollowRedirectAsync(patchResponse);
            var result = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Zone>();

            // Then
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(firstZone.Routes.Count + 1, result.Routes.Count);
            var addedRoute = result.Routes["UpdatedAlias"];
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
