using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Documents.Queries
{
    public class GetContentById
    {
        public class Query
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Query>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpGet("/Documents/{Id:int}/Content", Name = "GetDocumentContentById")]
            public override async Task<ActionResult> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Documents
                    .Where(x => x.ProjectId == projectId && x.Id == request.Id)
                    .Select(x => new { x.Content, x.ContentType });
                var result = await query.SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                return File(result.Content, result.ContentType);
            }
        }
    }
}
