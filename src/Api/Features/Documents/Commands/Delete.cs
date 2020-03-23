using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Documents.Commands
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

            [HttpDelete("/Documents/{Id:int}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var document = await _db.Document
                    .Where(x => x.ProjectId == projectId)
                    .SingleOrDefaultAsync(x => x.Id == request.Id);

                if (document is null)
                {
                    return NotFound();
                }

                _db.Document.Remove(document);
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
