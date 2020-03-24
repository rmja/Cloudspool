using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Resources.Commands
{
    public class Set
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

            [HttpPut("/Resources/{Alias}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                using var content = new MemoryStream();
                await Request.Body.CopyToAsync(content);

                var resource = await _db.Resource.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Alias == request.Alias);

                if (resource is object)
                {
                    resource.Content = content.ToArray();
                    resource.ContentType = Request.ContentType;
                }
                else
                {
                    resource = new DataModels.Resource(projectId, request.Alias, content.ToArray(), Request.ContentType);
                    _db.Resource.Add(resource);
                }

                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
