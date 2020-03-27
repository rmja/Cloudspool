using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PrintSpooler.Proxy
{
    public class PrintingHubProxy : IAsyncDisposable
    {
        private const string AuthorizationHeaderName = "Authorization";

        private readonly HubConnection _hub;
        private readonly IDisposable[] _subscriptions;
        private readonly IOptions<PrintSpoolerOptions> _options;
        private readonly ILogger<PrintingHubProxy> _logger;

        public Func<Task> OnRequestInstalledPrintersAsync { get; set; }
        public Func<SpoolerJob, Task> OnSpoolJobAsync { get; set; }

        public bool IsConnected => _hub.ConnectionId is object;

        public PrintingHubProxy(IOptions<PrintSpoolerOptions> options, ILogger<PrintingHubProxy> logger)
        {
            _options = options;
            _logger = logger;

            var spoolerKey = options.Value.SpoolerKey;

            var hubUrl = new Uri(options.Value.PrintingHubUrl);

            _hub = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.Headers.Add(AuthorizationHeaderName, $"Bearer spooler:{spoolerKey}");

                    if (hubUrl.Host.EndsWith("localhost"))
                    {
                        logger.LogWarning("Using insecure certificate validation for localhost");

                        options.HttpMessageHandlerFactory = handler =>
                        {
                            if (handler is HttpClientHandler clientHandler)
                            {
                                clientHandler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                            }

                            return handler;
                        };
                    }
                })
                .WithAutomaticReconnect()
                .Build();

            _subscriptions = new[]
            {
                _hub.On("RequestInstalledPrinters", () => OnRequestInstalledPrintersAsync?.Invoke() ?? Task.CompletedTask),
                _hub.On("SpoolJob", (SpoolerJob job) => OnSpoolJobAsync?.Invoke(job) ?? Task.CompletedTask)
            };
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting connection to {PrintingHub} using spooler key {SpoolerKey}", _options.Value.PrintingHubUrl, _options.Value.SpoolerKey);
            return _hub.StartAsync(cancellationToken);
        }

        public Task Hello(string version, CancellationToken cancellationToken = default) => _hub.InvokeAsync("Hello", version, cancellationToken);
        public Task Heartbeat(CancellationToken cancellationToken = default) => _hub.InvokeAsync("Heartbeat", cancellationToken);
        public Task SetInstalledPrintersAsync(string[] printerNames, CancellationToken cancellationToken = default) => _hub.InvokeAsync("SetInstalledPrinters", printerNames, cancellationToken);
        public Task SpoolPendingJobsAsync(CancellationToken cancellationToken = default) =>  _hub.InvokeAsync("SpoolPendingJobs", cancellationToken);

        public async ValueTask DisposeAsync()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            await _hub.DisposeAsync();
        }
    }
}
