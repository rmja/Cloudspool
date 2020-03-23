using Intercom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Jobs.Commands
{
    public class PrintRaw
    {
        public class Command
        {
            public int SpoolerId { get; set; }
            [Required]
            public string PrinterName { get; set; }
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

            [HttpPost("/Spoolers/{SpoolerId:int}/Print")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                if (!await _db.Spooler.AnyAsync(x => x.Zone.ProjectId == projectId && x.Id == request.SpoolerId))
                {
                    return NotFound();
                }

                using var body = new MemoryStream();
                await Request.Body.CopyToAsync(body);

                var job = new PrintJob()
                {
                    PrinterName = request.PrinterName,
                    ContentType = Request.ContentType,
                    Content = body.ToArray()
                };

                await PrintHelpers.QueuePrintJobAsync(_redis, request.SpoolerId, job);

                return Accepted();
            }
        }
    }
}
