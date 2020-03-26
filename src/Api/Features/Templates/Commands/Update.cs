using Cloudspool.Api.Client.Models;
using Api.Features.Templates.Queries;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Templates.Commands
{
    public class Update
    {
        public class Command
        {
            public int Id { get; set; }
            [FromBody]
            public JsonPatchDocument<Template> Patch { get; set; }
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

            [HttpPatch("/Templates/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var template = await _db.Templates.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.Id);
                
                if (template is null)
                {
                    return NotFound();
                }

                var patched = _mapper.Patch(template, request.Patch);
                template.Name = patched.Name;
                await _db.SaveChangesAsync();

                return SeeOtherEndpoint(new GetById.Query() { Id = template.Id });
            }
        }
    }
}
