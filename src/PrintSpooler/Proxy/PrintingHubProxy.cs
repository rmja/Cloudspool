using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrintSpooler.Proxy
{
    public class PrintingHubProxy : IAsyncDisposable
    {
        private const string AuthorizationHeaderName = "Authorization";

        private readonly HubConnection _hub;
        private readonly IDisposable _onSpoolerJobSubscription;

        public event SpoolJobEventHandler OnSpoolJob;

        public PrintingHubProxy(IOptions<PrintSpoolerOptions> options)
        {
            var spoolerKey = options.Value.SpoolerKey;

            _hub = new HubConnectionBuilder()
                .WithUrl("https://localhost:51332/Printing", options => options.Headers.Add(AuthorizationHeaderName, $"Bearer spooler:{spoolerKey}"))
                .WithAutomaticReconnect()
                .Build();

            _onSpoolerJobSubscription = _hub.On("SpoolJob", (SpoolerJob job) => OnSpoolJob?.Invoke(job));
        }

        public Task StartAsync(CancellationToken cancellationToken = default) => _hub.StartAsync(cancellationToken);

        public Task RegisterPrintersAsync(string[] printerNames, CancellationToken cancellationToken = default) => _hub.InvokeAsync("RegisterPrinters", printerNames, cancellationToken);

        public Task SpoolPendingJobsAsync(CancellationToken cancellationToken = default) =>  _hub.InvokeAsync("SpoolPendingJobs", cancellationToken);

        public async ValueTask DisposeAsync()
        {
            _onSpoolerJobSubscription.Dispose();
            await _hub.DisposeAsync();
        }
    }

    public delegate void SpoolJobEventHandler(SpoolerJob job);
}
