using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Templates.Queries
{
    public class GetScriptById
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

            [HttpGet("/Templates/{Id:int}/Script", Name = RouteNames.GetTemplateScriptById)]
            public override async Task<ActionResult> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Template
                    .Where(x => x.ProjectId == projectId && x.Id == request.Id)
                    .Select(x => new { x.Script, x.ScriptContentType });
                var result = await query.SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                return Content(result.Script, result.ScriptContentType);
            }
        }
    }
}
