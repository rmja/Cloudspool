using Intercom;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Features.Jobs.Commands
{
    public static class PrintHelpers
    {
        public static async Task QueuePrintJobAsync(ConnectionMultiplexer redis, PrintJobRequest job)
        {
            var db = redis.GetDatabase();
            var subscriber = redis.GetSubscriber();

            var queue = RedisConstants.Queues.PrintJobQueue(job.SpoolerId);
            var jobJson = JsonSerializer.Serialize(job);
            await db.ListRightPushAsync(queue, jobJson);
            await subscriber.PublishAsync(RedisConstants.Channels.JobCreated, queue);
        }
    }
}
