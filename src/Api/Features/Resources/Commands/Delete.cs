using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Resources.Commands
{
    public class Delete
    {
        public class Command
        {
            public string Alias { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpDelete("/Resources/{Alias}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var resource = await _db.Resource.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Alias == request.Alias);

                if (resource is null)
                {
                    return NotFound();
                }

                _db.Resource.Remove(resource);
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
