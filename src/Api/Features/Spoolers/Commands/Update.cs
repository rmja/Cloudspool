using Api.Client.Models;
using Api.Features.Spoolers.Queries;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Commands
{
    public class Update
    {
        public class Command
        {
            public int Id { get; set; }
            [FromBody]
            public JsonPatchDocument<Spooler> Patch { get; set; }
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

            [HttpPatch("/Spoolers/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var spooler = await _db.Spoolers.SingleOrDefaultAsync(x => x.Zone.ProjectId == projectId && x.Id == request.Id);
                
                if (spooler is null)
                {
                    return NotFound();
                }

                var patched = _mapper.Patch(spooler, request.Patch);
                spooler.Name = patched.Name;
                await _db.SaveChangesAsync();

                return SeeOtherEndpoint(new GetById.Query() { Id = spooler.Id });
            }
        }
    }
}
