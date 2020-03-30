using Api.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Templates.Commands
{
    public class SetScript
    {
        public class Command
        {
            public int Id { get; set; }
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

            [HttpPut("/Templates/{Id:int}/Script")]
            [Consumes("application/javascript", "application/typescript")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var template = await _db.Templates.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.Id);
                
                if (template is null)
                {
                    return NotFound();
                }

                var mediaType = Request.GetTypedHeaders().ContentType.MediaType.Value;

                using var reader = new StreamReader(Request.Body);
                var script = await reader.ReadToEndAsync();

                var generator = _generatorProvider.GetGenerator(mediaType);
                var errors = generator.ValidateTemplate(script);

                if (errors.Length > 0)
                {
                    return BadRequest(errors);
                }

                template.Script = script;
                template.ScriptMediaType = mediaType;
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
