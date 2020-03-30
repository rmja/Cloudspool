using Api.DataModels;
using Api.Features.Templates.Queries;
using Api.Generators;
using Api.Generators.ECMAScript6;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Templates.Commands
{
    public class Create
    {
        public class Command
        {
            [Required]
            public string Name { get; set; }
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

            [HttpPost("/Templates")]
            [Consumes("application/javascript", "application/typescript")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                using var reader = new StreamReader(Request.Body);
                var script = await reader.ReadToEndAsync();

                var mediaType = Request.GetTypedHeaders().ContentType.MediaType.Value;

                var generator = _generatorProvider.GetGenerator(mediaType);
                var errors = generator.ValidateTemplate(script);

                if (errors.Length > 0)
                {
                    return BadRequest(errors);
                }

                var template = new Template(projectId, request.Name, script, mediaType);
                _db.Templates.Add(template);
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = template.Id });
            }
        }
    }
}
