using Intercom;
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
    public class QueueProcessor : BackgroundService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IHubContext<PrintingHub, ISpoolerClient> _printingHubContext;
        private readonly BlockingCollection<string> _signal = new BlockingCollection<string>();

        public QueueProcessor(ConnectionMultiplexer redis, IHubContext<PrintingHub, ISpoolerClient> printingHubContext)
        {
            _redis = redis;
            _printingHubContext = printingHubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();
            var subscriber = _redis.GetSubscriber();
            
            await subscriber.SubscribeAsync(RedisConstants.Channels.JobCreated, JobScheduled);

            foreach (var queue in _signal.GetConsumingEnumerable())
            {
                var requestJson = await db.ListLeftPopAsync(queue);

                if (requestJson.HasValue)
                {
                    try
                    {
                        if (queue.EndsWith(":print-job-queue"))
                        {
                            var request = JsonSerializer.Deserialize<PrintJobRequest>(requestJson);
                            await HandlePrintJobAsync(request);
                        }
                        else if (queue.EndsWith(":get-installed-printers-queue"))
                        {
                            var request = JsonSerializer.Deserialize<RequestInstalledPrintersRefreshRequest>(requestJson);
                            await HandleGetInstalledPrintersAsync(request);
                        }
                        else
                        {
                            throw new NotSupportedException("Invalid queue name");
                        }
                    }
                    catch (Exception)
                    {
                        await db.ListRightPushAsync(queue, requestJson);
                        throw;
                    }
                }
            }
        }

        public void QueuePendingJobs(int spoolerId)
        {
            _signal.Add(RedisConstants.Queues.PrintJobQueue(spoolerId));
        }

        private void JobScheduled(RedisChannel channel, RedisValue message)
        {
            _signal.Add(message);
        }

        private async Task HandleGetInstalledPrintersAsync(RequestInstalledPrintersRefreshRequest request)
        {
            var db = _redis.GetDatabase();
            var connectionId = await db.HashGetAsync(RedisConstants.ConnectedClients, request.SpoolerId);

            if (connectionId.HasValue)
            {
                var spoolerClient = _printingHubContext.Clients.Client(connectionId);
                await spoolerClient.RequestInstalledPrinters();
            }
        }

        private async Task HandlePrintJobAsync(PrintJobRequest request)
        {
            var db = _redis.GetDatabase();
            var connectionId = await db.HashGetAsync(RedisConstants.ConnectedClients, request.SpoolerId);

            if (connectionId.HasValue)
            {
                var spoolerClient = _printingHubContext.Clients.Client(connectionId);
                await spoolerClient.SpoolJob(new SpoolerJob()
                {
                    PrinterName = request.PrinterName,
                    ContentType = request.ContentType,
                    Content = request.Content
                });
            }
            else
            {
                throw new Exception("Spooler is not connected"); // This effectively re-adds the job to the queue
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _signal.CompleteAdding();

            var subscriber = _redis.GetSubscriber();
            await subscriber.UnsubscribeAsync(RedisConstants.Channels.JobCreated, JobScheduled);
            await base.StopAsync(cancellationToken);
        }
    }
}
