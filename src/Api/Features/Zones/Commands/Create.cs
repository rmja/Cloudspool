using Api.DataModels;
using Api.Features.Zones.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Zones.Commands
{
    public class Create
    {
        public class Command
        {
            [FromBody]
            public Body Body { get; set; }
        }

        public class Body
        {
            [Required]
            public string Name { get; set; }
            public Dictionary<string, Route> Routes { get; set; } = new Dictionary<string, Route>();

            public class Route
            {
                [Required]
                public int SpoolerId { get; set; }
                [Required]
                public string PrinterName { get; set; }
            }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpPost("/Zones")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var zone = new Zone(projectId, request.Body.Name);

                foreach (var (alias, route) in request.Body.Routes)
                {
                    zone.Routes.Add(new ZoneRoute(alias)
                    {
                        SpoolerId = route.SpoolerId,
                        PrinterName = route.PrinterName
                    });
                }

                if (!await CommandHelpers.HasValidRoutes(zone, _db, projectId))
                {
                    return BadRequest();
                }

                _db.Zones.Add(zone);
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = zone.Id });
            }
        }
    }
}
