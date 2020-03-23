using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using StackExchange.Redis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Features
{
    public class JobsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ConnectionMultiplexer _redis;

        public JobsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.WithPopulatedSeedData().CreateClient();
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer project:{SeedData.TestProjectKey}");

            _redis = factory.Services.GetRequiredService<ConnectionMultiplexer>();
        }

        [Fact]
        public async Task CanPrintRaw()
        {
            // Given
            var firstSpooler = SeedData.GetSpoolers().First();
            var db = _redis.GetDatabase();
            var subscriber = _redis.GetSubscriber();

            await db.KeyDeleteAsync($"spoolers:{firstSpooler.Id}:job-queue");
            var jobCreatedQueue = await subscriber.SubscribeAsync("job-created");

            var content = new StringContent("This is Epson", Encoding.UTF8, "application/escpos");

            // When
            var response = await _client.PostAsync($"/Spoolers/{firstSpooler.Id}/Print?PrinterName=SlipPrinter", content);
            response.EnsureSuccessStatusCode();

            // Then
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            var jobQueue = await db.ListRangeAsync($"spoolers:{firstSpooler.Id}:job-queue");
            Assert.Single(jobQueue);
            var message = await jobCreatedQueue.ReadAsync();
            Assert.Equal(firstSpooler.Id, message.Message);

            jobCreatedQueue.Unsubscribe();
        }
    }
}
