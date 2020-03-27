using Cloudspool.Api.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Dispatcher
{
    [Authorize]
    public class PrintingHub : Hub<ISpoolerClient>
    {
        private readonly IApiClient _api;
        private readonly ConnectionMultiplexer _redis;
        private readonly QueueProcessor _printJobProcessor;
        private readonly ILogger<PrintingHub> _logger;

        public PrintingHub(IApiClient api, ConnectionMultiplexer redis, QueueProcessor printJobProcessor, ILogger<PrintingHub> logger)
        {
            _api = api;
            _redis = redis;
            _printJobProcessor = printJobProcessor;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var spoolerId = GetSpoolerId();
            var db = _redis.GetDatabase();
            await db.HashSetAsync(RedisConstants.ConnectedClients, spoolerId, Context.ConnectionId);
            await db.HashSetAsync(RedisConstants.SpoolerDetails(spoolerId), "last-connected", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var spoolerId = GetSpoolerId();
            var db = _redis.GetDatabase();
            await db.HashDeleteAsync(RedisConstants.ConnectedClients, spoolerId);
            await db.HashSetAsync(RedisConstants.SpoolerDetails(spoolerId), "last-disconnected", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Hello(string version)
        {
            var spoolerId = GetSpoolerId();
            var db = _redis.GetDatabase();
            var entries = new HashEntry[]
            {
                new HashEntry("last-hello-received", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)),
                new HashEntry("print-spooler-app-version", version)
            };
            await db.HashSetAsync(RedisConstants.SpoolerDetails(spoolerId), entries);
        }

        public async Task Heartbeat()
        {
            var spoolerId = GetSpoolerId();
            var db = _redis.GetDatabase();
            await db.HashSetAsync(RedisConstants.SpoolerDetails(spoolerId), "last-heartbeat-received", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        }

        public async Task SetInstalledPrinters(string[] printerNames)
        {
            var spoolerId = GetSpoolerId();
            _logger.LogInformation("Spooler {SpoolerId} is registering the printers: {PrinterNames}.", spoolerId, string.Join(", ", printerNames));
            await _api.SpoolerSetPrintersAsync(spoolerId, printerNames);
        }

        public async Task SpoolPendingJobs()
        {
            var spoolerId = GetSpoolerId();
            _logger.LogInformation("Spooler {SpoolerId} is requesting to spool its pending jobs.", spoolerId);

            _printJobProcessor.QueuePendingJobs(spoolerId);
            var db = _redis.GetDatabase();
            await db.HashSetAsync(RedisConstants.SpoolerDetails(spoolerId), "last-job-spooled", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        }

        private int GetSpoolerId() => int.Parse(Context.User.FindFirst("SpoolerId").Value);
    }
}
