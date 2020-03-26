using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
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

        public Func<Task> OnRequestInstalledPrintersAsync { get; set; }
        public Func<SpoolerJob, Task> OnSpoolJobAsync { get; set; }

        public PrintingHubProxy(IHostEnvironment env, IOptions<PrintSpoolerOptions> options)
        {
            var spoolerKey = options.Value.SpoolerKey;

            _hub = new HubConnectionBuilder()
                .WithUrl(options.Value.PrintingHubUrl, options =>
                {
                    options.Headers.Add(AuthorizationHeaderName, $"Bearer spooler:{spoolerKey}");

                    if (env.IsDevelopment())
                    {
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

        public Task StartAsync(CancellationToken cancellationToken = default) => _hub.StartAsync(cancellationToken);

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
