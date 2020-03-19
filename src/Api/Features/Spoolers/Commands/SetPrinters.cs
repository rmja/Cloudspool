using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Commands
{
    public class SetPrinters
    {
        public class Command
        {
            public int SpoolerId { get; set; }
            [FromBody]
            public string[] PrinterNames { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly ConnectionMultiplexer _redis;

            public Handler(ConnectionMultiplexer redis)
            {
                _redis = redis;
            }

            [HttpPut("/Spoolers/{SpoolerId:int}/Printers")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var db = _redis.GetDatabase();

                var printerNamesKey = $"spoolers:{request.SpoolerId}:printers";
                var printerNames = request.PrinterNames.Select(x => (RedisValue)x).ToArray();

                var multi = db.CreateTransaction();
                _ = multi.KeyDeleteAsync(printerNamesKey, CommandFlags.FireAndForget);
                _ = multi.SetAddAsync(printerNamesKey, printerNames, CommandFlags.FireAndForget);
                await multi.ExecuteAsync();

                return Ok();
            }
        }
    }
}
