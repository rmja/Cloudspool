using Api.DataModels;
using Api.Features.Documents.Queries;
using Api.Generators;
using Api.Generators.JavaScript;
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
            private readonly GeneratorProvider _generatorProvider;

            public Handler(CloudspoolContext db, GeneratorProvider generatorProvider)
            {
                _db = db;
                _generatorProvider = generatorProvider;
            }

            [HttpPost("/Zones/{ZoneId:int}/Documents/Generate")]
            [HttpPost("/Documents/Generate")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                if (request.ZoneId == 0)
                {
                    if (User.TryGetZoneId(out var zoneId))
                    {
                        request.ZoneId = zoneId;
                    }
                    else
                    {
                        return Forbid();
                    }
                }

                var format = await _db.Formats
                    .Where(x => x.Zone.ProjectId == projectId && x.ZoneId == request.ZoneId)
                    .Select(x => new
                    {
                        x.Alias,
                        x.TemplateId,
                        x.Template.Script,
                        x.Template.ScriptMediaType
                    })
                    .SingleOrDefaultAsync(x => x.Alias == request.Format);

                if (format is null)
                {
                    return BadRequest("Invalid format");
                }

                var generator = _generatorProvider.GetGenerator(format.ScriptMediaType);
                var resources = new DbResourceManager(_db, User.GetProjectId());
                var result = await generator.GenerateDocumentAsync(format.Script, request.Model, resources);
                var document = new Document(User.GetProjectId(), format.TemplateId, result.Content, result.ContentType);
                _db.Documents.Add(document);
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
                return _db.Resources
                    .Where(x => x.ProjectId == _projectId && x.Alias == alias)
                    .Select(x => x.Content)
                    .SingleOrDefault();
            }

            public Task<byte[]> GetResourceAsync(string alias, CancellationToken cancellationToken = default)
            {
                return _db.Resources
                    .Where(x => x.ProjectId == _projectId && x.Alias == alias)
                    .Select(x => x.Content)
                    .SingleOrDefaultAsync(cancellationToken);
            }
        }
    }
}
