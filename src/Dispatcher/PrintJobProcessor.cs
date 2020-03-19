using Intercom;
using Intercom.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher
{
    public class PrintJobProcessor : BackgroundService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IHubContext<PrintingHub> _printingHubContext;
        private readonly BlockingCollection<int> _signal = new BlockingCollection<int>();

        public PrintJobProcessor(ConnectionMultiplexer redis, IHubContext<PrintingHub> printingHubContext)
        {
            _redis = redis;
            _printingHubContext = printingHubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();
            var subscriber = _redis.GetSubscriber();
            
            await subscriber.SubscribeAsync("job-created", JobScheduled);

            foreach (var spoolerId in _signal.GetConsumingEnumerable())
            {
                var queueName = $"spoolers:{spoolerId}:job-queue";
                var jobJson = await db.ListLeftPopAsync(queueName);

                if (jobJson.HasValue)
                {
                    try
                    {
                        var job = JsonSerializer.Deserialize<PrintJob>(jobJson);
                        await HandleJobAsync(spoolerId, job);
                    }
                    catch (Exception)
                    {
                        await db.ListRightPushAsync(queueName, jobJson);
                        throw;
                    }
                }
            }
        }

        public void QueuePendingJobs(int spoolerId)
        {
            _signal.Add(spoolerId);
        }

        private void JobScheduled(RedisChannel channel, RedisValue message)
        {
            _signal.Add((int)message);
        }

        private async Task HandleJobAsync(int spoolerId, PrintJob job)
        {
            var spoolerClient = _printingHubContext.Clients.Group(spoolerId.ToString());
            
            await spoolerClient.SendAsync("SpoolJob", new SpoolerJob()
            {
                PrinterName = job.PrinterName,
                ContentType = job.ContentType,
                Content = job.Content
            });
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _signal.CompleteAdding();

            var subscriber = _redis.GetSubscriber();
            await subscriber.UnsubscribeAsync("job-created", JobScheduled);
            await base.StopAsync(cancellationToken);
        }
    }
}
