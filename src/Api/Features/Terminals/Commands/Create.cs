using Api.DataModels;
using Api.Features.Terminals.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Terminals.Commands
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
            public List<Route> Routes { get; set; } = new List<Route>();

            public class Route
            {
                [Required]
                public string Alias { get; set; }
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

            [HttpPost("/Zones/{ZoneId:int}/Terminals")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                if (!await _db.Zone.AnyAsync(x => x.ProjectId == projectId && x.Id == request.ZoneId))
                {
                    return NotFound();
                }

                var terminal = new Terminal(request.ZoneId, request.Body.Name);

                foreach (var route in request.Body.Routes)
                {
                    terminal.AddRoute(route.Alias, route.SpoolerId, route.PrinterName);
                }

                if (!await CommandHelpers.HasValidRoutes(terminal, _db, projectId))
                {
                    return BadRequest();
                }

                _db.Terminal.Add(terminal);
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = terminal.Id });
            }
        }
    }
}
