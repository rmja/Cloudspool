using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrintSpooler.Proxy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrintSpooler
{
    public class HeartbeatWorker : BackgroundService
    {
        private readonly PrintingHubProxy _printingHubProxy;
        private readonly ILogger<HeartbeatWorker> _logger;

        public HeartbeatWorker(PrintingHubProxy printingHubProxy, ILogger<HeartbeatWorker> logger)
        {
            _printingHubProxy = printingHubProxy;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_printingHubProxy.IsConnected)
                {
                    try
                    {
                        await _printingHubProxy.Heartbeat(stoppingToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Heartbeat failed");
                    }
                }

                await Task.Delay(60_000, stoppingToken);
            }
        }
    }
}
