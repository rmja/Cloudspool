using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Terminals.Commands
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

            [HttpDelete("/Terminals/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var terminal = await _db.Terminals.SingleOrDefaultAsync(x => x.Zone.ProjectId == projectId && x.Id == request.Id);

                if (terminal is null)
                {
                    return NotFound();
                }

                _db.Terminals.Remove(terminal);
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
