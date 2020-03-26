using Cloudspool.Api.Client;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.Options;
using Refit;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests
{
    public class ClientTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IApiClient _client;
        private string _key;

        public ClientTests(CustomWebApplicationFactory factory)
        {
            var options = new ApiClientOptions()
            {
                GetApiKeyAsync = sp => Task.FromResult(_key)
            };

            var client = factory.WithPopulatedSeedData().CreateDefaultClient(
                new RedirectHandler(),
                new AuthorizationMessageHandler(factory.Services, Options.Create(options)));

            _client = RestService.For<IApiClient>(client);
        }

        [Fact]
        public async Task CanGenerateAndPrintDocument()
        {
            // Given
            var terminal = SeedData.GetTerminals().Single(x => x.Key == SeedData.TestTerminalKey);
            terminal.Zone = SeedData.GetZones().Single(x => x.Id == terminal.ZoneId);
            var firstRoute = terminal.Zone.Routes.First();
            _key = $"terminal:{terminal.Key}";

            var document = await _client.DocumentsGenerateAsync("slip", new
            {
                name = "Rasmus"
            });

            await _client.JobsPrintDocumentAsync(document.Id, firstRoute.Alias);
        }
    }
}
