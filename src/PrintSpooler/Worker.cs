using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrintSpooler.Proxy;
using PrintSpooler.Windows.Printing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrintSpooler
{
    public class Worker : BackgroundService
    {
        private readonly PrintingHubProxy _printingHubProxy;
        private readonly ILogger<Worker> _logger;

        public Worker(PrintingHubProxy printingHubProxy, ILogger<Worker> logger)
        {
            _printingHubProxy = printingHubProxy;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _printingHubProxy.StartAsync(stoppingToken);

            var printers = Printers.GetInstalledPrinters();

            _logger.LogInformation("Reporting {PrinterCount} printers to dispatcher", printers.Length);
            await _printingHubProxy.RegisterPrintersAsync(printers, stoppingToken);

            _printingHubProxy.OnSpoolJob += _printingHubProxy_OnSpoolerJob;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void _printingHubProxy_OnSpoolerJob(SpoolerJob job)
        {
            throw new NotImplementedException();
        }
    }
}
