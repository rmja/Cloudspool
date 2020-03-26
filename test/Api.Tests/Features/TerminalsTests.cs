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
    public class TerminalsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TerminalsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");
        }

        [Fact]
        public async Task CanCreate()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            var firstSpooler = SeedData.GetSpoolers().First();
            var command = new Terminal()
            {
                Name = "Test Terminal",
                Routes =
                {
                    ["TerminalRoute"] = new Terminal.Route() { SpoolerId = firstSpooler.Id, PrinterName = "Test Printer" }
                }
            };

            // When
            var response = await _client.PostAsJsonAsync($"/Zones/{firstZone.Id}/Terminals", command);
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Terminal>();

            // Then
            Assert.Equal(SeedData.TestProject.Id, result.ProjectId);
            Assert.Equal(firstZone.Id, result.ZoneId);
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
            var firstTerminal = SeedData.GetTerminals().First();

            // When
            var response = await _client.DeleteAsync($"/Terminals/{firstTerminal.Id}");
            response.EnsureSuccessStatusCode();

            // Then
        }

        [Fact]
        public async Task CanUpdate()
        {
            // Given
            var firstTerminal = SeedData.GetTerminals().First();
            var firstSpooler = SeedData.GetSpoolers().First();
            var patch = new JsonPatchDocument<Terminal>()
                .Replace(x => x.Name, "Updated Name");
            patch.Operations.Add(new Operation<Terminal>("add", "/routes/UpdatedAlias", null, new Terminal.Route() { SpoolerId = firstSpooler.Id, PrinterName = "Updated Printer" }));

            // When
            var patchResponse = await _client.PatchAsJsonAsync($"/Terminals/{firstTerminal.Id}", patch.Operations);
            var getResponse = await _client.FollowRedirectAsync(patchResponse);
            var result = await getResponse.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Terminal>();

            // Then
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(firstTerminal.Routes.Count + 1, result.Routes.Count);
            var addedRoute = result.Routes["UpdatedAlias"];
            Assert.Equal(firstSpooler.Id, addedRoute.SpoolerId);
            Assert.Equal("Updated Printer", addedRoute.PrinterName);
        }

        [Fact]
        public async Task CanGetAllByZoneId()
        {
            // Given
            var firstZone = SeedData.GetZones().First();
            firstZone.Terminals = SeedData.GetTerminals().Where(x => x.ZoneId == firstZone.Id).ToList();

            // When
            var response = await _client.GetAsync($"/Zones/{firstZone.Id}/Terminals");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<List<Terminal>>();

            // Then
            Assert.Equal(firstZone.Terminals.Count, result.Count);
        }

        [Fact]
        public async Task CanGetById()
        {
            // Given
            var firstTerminal = SeedData.GetTerminals().First();
            firstTerminal.Zone = SeedData.GetZones().First(x => x.Id == firstTerminal.ZoneId);

            // When
            var response = await _client.GetAsync($"/Terminals/{firstTerminal.Id}");
            var result = await response.EnsureSuccessStatusCode().Content.ReadAsJsonAsync<Terminal>();

            // Then
            Assert.Equal(firstTerminal.Id, result.Id);
            Assert.Equal(firstTerminal.Zone.ProjectId, result.ProjectId);
            Assert.Equal(firstTerminal.Routes.Count, result.Routes.Count);
        }
    }
}
