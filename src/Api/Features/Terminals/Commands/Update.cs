using Api.Client.Models;
using Api.Features.Terminals.Queries;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Terminals.Commands
{
    public class Update
    {
        public class Command
        {
            public int Id { get; set; }
            [FromBody]
            public JsonPatchDocument<Terminal> Patch { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpPatch("/Terminals/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var terminal = await _db.Terminal.Include(x => x.Routes).SingleOrDefaultAsync(x => x.Zone.ProjectId == projectId && x.Id == request.Id);
                
                if (terminal is null)
                {
                    return NotFound();
                }

                var patched = _mapper.Patch(terminal, request.Patch);
                terminal.Name = patched.Name;

                terminal.Routes.Clear();
                foreach (var route in patched.Routes)
                {
                    terminal.AddRoute(route.Alias, route.SpoolerId, route.PrinterName);
                }

                if (!await CommandHelpers.HasValidRoutes(terminal, _db, projectId))
                {
                    return BadRequest();
                }

                await _db.SaveChangesAsync();

                return RedirectToRoute(RouteNames.GetTerminalById, new GetById.Query() { Id = terminal.Id });
            }
        }
    }
}
