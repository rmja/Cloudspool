using Intercom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Jobs.Commands
{
    public class PrintDocument
    {
        public class Command
        {
            public int DocumentId { get; set; }
            [Required]
            public string Route { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;
            private readonly ConnectionMultiplexer _redis;

            public Handler(CloudspoolContext db, ConnectionMultiplexer connection)
            {
                _db = db;
                _redis = connection;
            }

            [HttpPost("/Documents/{DocumentId:int}/Print")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var document = await _db.Documents.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.DocumentId);

                if (document is null)
                {
                    return NotFound();
                }

                var route = await GetPrinterByTerminal(projectId, request.Route);

                if (route is null)
                {
                    route = await GetPrinterByZone(projectId, request.Route);
                }

                if (route is null)
                {
                    return BadRequest("Printer not found from route");
                }

                var job = new PrintJobRequest()
                {
                    SpoolerId = route.SpoolerId,
                    PrinterName = route.PrinterName,
                    ContentType = document.ContentType,
                    Content = document.Content
                };

                await PrintHelpers.QueuePrintJobAsync(_redis, job);

                return Accepted();
            }

            private async Task<PrinterRoute> GetPrinterByTerminal(int projectId, string routeAlias)
            {
                var route = await _db.Terminals
                    .SelectMany(x => x.Routes)
                    .Where(x => x.Terminal.Zone.ProjectId == projectId && x.Alias == routeAlias)
                    .SingleOrDefaultAsync();

                if (route is null)
                {
                    return null;
                }

                return new PrinterRoute() { SpoolerId = route.SpoolerId, PrinterName = route.PrinterName };
            }

            private async Task<PrinterRoute> GetPrinterByZone(int projectId, string routeAlias)
            {
                var route = await _db.Zones
                        .SelectMany(x => x.Routes)
                        .Where(x => x.Zone.ProjectId == projectId && x.Alias == routeAlias)
                        .SingleOrDefaultAsync();

                if (route is null)
                {
                    return null;
                }

                return new PrinterRoute() { SpoolerId = route.SpoolerId, PrinterName = route.PrinterName };
            }

            class PrinterRoute
            {
                public int SpoolerId { get; set; }
                public string PrinterName { get; set; }
            }
        }
    }
}
