using Api.DataModels;
using Api.Features.Documents.Queries;
using Api.Generators.ECMAScript6;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Documents.Commands
{
    public class Generate
    {
        public class Command
        {
            public int ZoneId { get; set; }
            [Required]
            public string Format { get; set; }
            [FromBody]
            public object Model { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;
            private readonly ECMAScript6Generator _ecmaScript6Generator;

            public Handler(CloudspoolContext db, ECMAScript6Generator ecmaScript6Generator)
            {
                _db = db;
                _ecmaScript6Generator = ecmaScript6Generator;
            }

            [HttpPost("/Zones/{ZoneId:int}/Documents/Generate")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var format = await _db.Format
                    .Where(x => x.ZoneId == request.ZoneId)
                    .Select(x => new
                    {
                        x.Alias,
                        x.TemplateId,
                        x.Template.Script
                    })
                    .SingleOrDefaultAsync(x => x.Alias == request.Format);

                if (format is null)
                {
                    return BadRequest("Invalid format");
                }

                var resources = new DbResourceManager(_db, User.GetProjectId());
                var (content, contentType) = await _ecmaScript6Generator.GenerateDocumentAsync(format.Script, request.Model, resources);

                var document = new Document(User.GetProjectId(), format.TemplateId, content, contentType);
                _db.Document.Add(document);
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = document.Id });
            }
        }

        class DbResourceManager : IResourceManager
        {
            private readonly CloudspoolContext _db;
            private readonly int _projectId;

            public DbResourceManager(CloudspoolContext db, int projectId)
            {
                _db = db;
                _projectId = projectId;
            }

            public byte[] GetResource(string alias)
            {
                return _db.Resource
                    .Where(x => x.ProjectId == _projectId && x.Alias == alias)
                    .Select(x => x.Content)
                    .SingleOrDefault();
            }

            public Task<byte[]> GetResourceAsync(string alias, CancellationToken cancellationToken)
            {
                return _db.Resource
                    .Where(x => x.ProjectId == _projectId && x.Alias == alias)
                    .Select(x => x.Content)
                    .SingleOrDefaultAsync();
            }
        }
    }
}
