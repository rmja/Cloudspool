using Api.DataModels;
using Api.Features.Spoolers.Queries;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Commands
{
    public class Create
    {
        public class Command
        {
            public int ZoneId { get; set; }
            [FromBody]
            public Body Body { get; set; }
        }

        public class Body
        {
            [Required]
            public string Name { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpPost("/Zones/{ZoneId:int}/Spoolers")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var spooler = new Spooler(request.ZoneId, request.Body.Name);
                _db.Spooler.Add(spooler);
                await _db.SaveChangesAsync();

                return RedirectToRoute(RouteNames.GetSpoolerById, new GetById.Query() { Id = spooler.Id});
            }
        }
    }
}
