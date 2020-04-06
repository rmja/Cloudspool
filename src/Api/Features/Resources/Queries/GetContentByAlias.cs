using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Resources.Queries
{
    public class GetContentByAlias
    {
        public class Query
        {
            public string Alias { get; set; }
        }

        public class Handler : ApiEndpoint<Query>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpGet("/Resources/{Alias}/Content", Name = "GetResourceContentByAlias")]
            public override async Task<ActionResult> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Resources
                    .Where(x => x.ProjectId == projectId && x.Alias == request.Alias)
                    .Select(x => new { x.Content, x.MediaType });
                var result = await query.SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                return File(result.Content, result.MediaType, fileDownloadName: request.Alias);
            }
        }
    }
}
