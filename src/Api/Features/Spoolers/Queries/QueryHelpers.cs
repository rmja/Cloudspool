using Cloudspool.Api.Client.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Queries
{
    public static class QueryHelpers
    {
        public static async Task GetPrintersAsync(Spooler spooler, IDatabase db)
        {
            var batch = db.CreateBatch();

            var isOnlineTask = batch.HashExistsAsync(RedisConstants.ConnectedClients, spooler.Id);
            var detailsTask = batch.HashGetAllAsync(RedisConstants.SpoolerDetails(spooler.Id));
            var printersTask = batch.SetMembersAsync(RedisConstants.InstalledPrinters(spooler.Id));

            batch.Execute();

            var isOnline = await isOnlineTask;
            var details = (await detailsTask).ToStringDictionary();
            var printers = await printersTask;

            spooler.IsOnline = isOnline;
            spooler.LastConnected = GetDateTimeOrNull(details.GetValueOrDefault("last-connected"));
            spooler.LastDisconnected = GetDateTimeOrNull(details.GetValueOrDefault("last-disconnected"));
            spooler.LastHelloReceived = GetDateTimeOrNull(details.GetValueOrDefault("last-hello-received"));
            spooler.LastHeartbeatReceived = GetDateTimeOrNull(details.GetValueOrDefault("last-heartbeat-received"));
            spooler.LastJobSpooled = GetDateTimeOrNull(details.GetValueOrDefault("last-job-spooled"));
            spooler.PrintSpoolerAppVersion = details.GetValueOrDefault("print-spooler-app-version");

            spooler.Printers = printers.Select(x => (string)x).ToArray();
        }

        private static DateTime? GetDateTimeOrNull(string value)
        {
            if (value is null)
            {
                return null;
            }

            return DateTime.ParseExact(value, "o", CultureInfo.InvariantCulture);
        }
    }
}
