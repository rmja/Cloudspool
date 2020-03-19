using Api.Client.Models;
using Api.Features.Zones.Queries;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Zones.Commands
{
    public class Update
    {
        public class Command
        {
            public int Id { get; set; }
            [FromBody]
            public JsonPatchDocument<Zone> Patch { get; set; }
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

            [HttpPatch("/Zones/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var zone = await _db.Zones.Include(x => x.Routes).SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.Id);
                
                if (zone is null)
                {
                    return NotFound();
                }

                var patched = _mapper.Patch(zone, request.Patch);
                zone.Name = patched.Name;

                zone.Routes.Clear();
                foreach (var route in patched.Routes)
                {
                    zone.AddRoute(route.Alias, route.SpoolerId, route.PrinterName);
                }

                if (!await CommandHelpers.HasValidRoutes(zone, _db, projectId))
                {
                    return BadRequest();
                }

                await _db.SaveChangesAsync();

                return RedirectToRoute(RouteNames.GetZoneById, new GetById.Query() { Id = zone.Id });
            }
        }
    }
}
