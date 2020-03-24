using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Templates.Commands
{
    public class Delete
    {
        public class Command
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpDelete("/Templates/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var template = await _db.Template.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.Id);

                if (template is null)
                {
                    return NotFound();
                }

                _db.Template.Remove(template);
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
