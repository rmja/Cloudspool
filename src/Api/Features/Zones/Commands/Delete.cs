using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Zones.Commands
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

            [HttpDelete("/Zones/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var zone = await _db.Zone.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.Id);

                if (zone is null)
                {
                    return NotFound();
                }

                _db.Zone.Remove(zone);
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
