using Api.Features.Templates.Queries;
using Api.Generators.ECMAScript6;
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
            private readonly ECMAScript6Generator _ecmaScript6Generator;

            public Handler(CloudspoolContext db, ECMAScript6Generator ecmaScript6Generator)
            {
                _db = db;
                _ecmaScript6Generator = ecmaScript6Generator;
            }

            [HttpPut("/Templates/{Id:int}/Script")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var template = await _db.Template.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.Id);
                
                if (template is null)
                {
                    return NotFound();
                }

                using var reader = new StreamReader(Request.Body);
                var script = await reader.ReadToEndAsync();

                var errors = _ecmaScript6Generator.ValidateTemplate(script);

                if (errors.Length > 0)
                {
                    return BadRequest(errors);
                }

                template.Script = script;
                template.ScriptContentType = Request.ContentType;
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = template.Id });
            }
        }
    }
}
