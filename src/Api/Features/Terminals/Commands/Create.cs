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

            [HttpPost("/Zones/{ZoneId:int}/Terminals")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                if (!await _db.Zones.AnyAsync(x => x.ProjectId == projectId && x.Id == request.ZoneId))
                {
                    return NotFound();
                }

                var terminal = new Terminal(request.ZoneId, request.Body.Name);

                foreach (var (alias, route) in request.Body.Routes)
                {
                    terminal.AddRoute(alias, route.SpoolerId, route.PrinterName);
                }

                if (!await CommandHelpers.HasValidRoutes(terminal, _db, projectId))
                {
                    return BadRequest();
                }

                _db.Terminals.Add(terminal);
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = terminal.Id });
            }
        }
    }
}
