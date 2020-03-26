using Cloudspool.Api.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Dispatcher
{
    [Authorize]
    public class PrintingHub : Hub
    {
        private readonly IApiClient _api;
        private readonly PrintJobProcessor _printJobProcessor;
        private readonly ILogger<PrintingHub> _logger;

        public PrintingHub(IApiClient api, PrintJobProcessor printJobProcessor, ILogger<PrintingHub> logger)
        {
            _api = api;
            _printJobProcessor = printJobProcessor;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, GetSpoolerId().ToString());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetSpoolerId().ToString());
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterPrinters(string[] printerNames)
        {
            var spoolerId = GetSpoolerId();
            _logger.LogInformation("Spooler {0} is registering the following printers: {1}.", spoolerId, string.Join(", ", printerNames));

            await _api.SpoolerSetPrintersAsync(spoolerId, printerNames);
        }

        public void SpoolPendingJobs()
        {
            var spoolerId = GetSpoolerId();
            _logger.LogInformation("Spooler {0} is requesting to spool its pending jobs.", spoolerId);

            _printJobProcessor.QueuePendingJobs(spoolerId);
        }

        private int GetSpoolerId() => int.Parse(Context.User.FindFirst("SpoolerId").Value);
    }
}
