using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            private readonly CloudspoolContext _db;
            private readonly ConnectionMultiplexer _redis;

            public Handler(CloudspoolContext db, ConnectionMultiplexer redis)
            {
                _db = db;
                _redis = redis;
            }

            [HttpPut("/Spoolers/{SpoolerId:int}/Printers")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                if (!await _db.Spoolers.AnyAsync(x => x.Zone.ProjectId == projectId && x.Id == request.SpoolerId))
                {
                    return NotFound();
                }

                var db = _redis.GetDatabase();
                var subscriber = _redis.GetSubscriber();
                var key = RedisConstants.InstalledPrinters(request.SpoolerId);
                var multi = db.CreateTransaction();
                _ = multi.KeyDeleteAsync(key, CommandFlags.FireAndForget);
                _ = multi.SetAddAsync(key, request.PrinterNames.Select(x => (RedisValue)x).ToArray(), CommandFlags.FireAndForget);
                await multi.ExecuteAsync();
                await subscriber.PublishAsync(RedisConstants.Channels.InstalledPrintersRefreshed(request.SpoolerId), string.Empty);

                return Ok();
            }
        }
    }
}
