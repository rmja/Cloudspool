using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

                var query = _db.Resource
                    .Where(x => x.ProjectId == projectId && x.Alias == request.Alias)
                    .Select(x => new { x.Content, x.MediaType });
                var result = await query.SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                var filename = request.Alias + GetExtension(result.MediaType);

                return File(result.Content, result.MediaType, filename);
            }

            private static string GetExtension(string mediaType)
            {
                switch (mediaType)
                {
                    case "application/json": return ".json";
                    case "image/bmp": return ".bmp";
                }

                throw new NotSupportedException();
            }
        }
    }
}
