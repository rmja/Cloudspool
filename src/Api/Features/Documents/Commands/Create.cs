using Api.DataModels;
using Api.Features.Documents.Queries;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Documents.Commands
{
    public class Create
    {
        public class Command
        {
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpPost("/Documents")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                using var body = new MemoryStream();
                await Request.Body.CopyToAsync(body);

                var document = new Document(User.GetProjectId(), body.ToArray(), Request.ContentType);
                _db.Documents.Add(document);
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = document.Id });
            }
        }
    }
}
