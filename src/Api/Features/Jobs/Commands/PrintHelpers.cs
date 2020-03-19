using Intercom.Models;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Features.Jobs.Commands
{
    public static class PrintHelpers
    {
        public static async Task QueuePrintJobAsync(ConnectionMultiplexer redis, int spoolerId, PrintJob job)
        {
            var db = redis.GetDatabase();
            var subscriber = redis.GetSubscriber();

            var jobJson = JsonSerializer.Serialize(job);

            await db.ListRightPushAsync($"spoolers:{spoolerId}:job-queue", jobJson);
            await subscriber.PublishAsync("job-created", spoolerId);
        }
    }
}
