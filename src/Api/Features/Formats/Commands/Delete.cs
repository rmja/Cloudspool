using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Formats.Commands
{
    public class Delete
    {
        public class Command
        {
            public int ZoneId { get; set; }
            public string Alias { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpDelete("/Zones/{ZoneId:int}/Formats/{Alias}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var format = await _db.Formats.SingleOrDefaultAsync(x => x.Zone.ProjectId == projectId && x.Alias == request.Alias);

                if (format is null)
                {
                    return NotFound();
                }

                _db.Formats.Remove(format);
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
