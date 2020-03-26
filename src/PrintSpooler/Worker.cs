using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrintSpooler.Printing;
using PrintSpooler.Printing.Ghostscript;
using PrintSpooler.Printing.Raw;
using PrintSpooler.Proxy;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PrintSpooler
{
    public class Worker : BackgroundService
    {
        private readonly PrintingHubProxy _printingHubProxy;
        private readonly ILogger<Worker> _logger;
        private readonly BlockingCollection<SpoolerJob> _queue = new BlockingCollection<SpoolerJob>();

        public Worker(PrintingHubProxy printingHubProxy, ILogger<Worker> logger)
        {
            _printingHubProxy = printingHubProxy;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _printingHubProxy.OnRequestInstalledPrintersAsync = async () =>
            {
                var printers = Printers.GetInstalledPrinters();
                _logger.LogInformation("Reporting {PrinterCount} printers to dispatcher", printers.Length);
                await _printingHubProxy.SetInstalledPrintersAsync(printers);
            };
            _printingHubProxy.OnSpoolJobAsync = job =>
            {
                _queue.Add(job);
                return Task.CompletedTask;
            };

            await _printingHubProxy.StartAsync(stoppingToken);

            var printers = Printers.GetInstalledPrinters();
            _logger.LogInformation("Reporting {PrinterCount} printers to dispatcher", printers.Length);
            await _printingHubProxy.SetInstalledPrintersAsync(printers);

            _logger.LogInformation("Starting job processing");
            foreach (var job in _queue.GetConsumingEnumerable(stoppingToken))
            {
                _logger.LogInformation("Got {JobSize} {ContentType} job for printer {PrinterName}", job.Content.Length, job.ContentType, job.PrinterName);

                if (TryGetPrinterHandle(job.PrinterName, job.ContentType, out var printerHandle))
                {
                    try
                    {
                        printerHandle.Print(job.Content);
                    }
                    catch (PrinterException e)
                    {
                        _logger.LogError(e, "Unable to print");
                    }
                }
                else
                {
                    _logger.LogWarning("Unable to get printer handle");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private static bool TryGetPrinterHandle(string printerName, string contentType, out IPrinterHandle printerHandle)
        {
            switch (contentType)
            {
                case "application/zpl":
                case "application/escp":
                case "application/starline":
                    printerHandle = new RawPrinterHandle(printerName);
                    return true;
                case "application/pdf":
                    printerHandle = new GhostscriptPrinterHandle(printerName);
                    return true;
                default:
                    printerHandle = default;
                    return false;
            }
        }
    }
}
