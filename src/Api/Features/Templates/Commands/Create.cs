using Api.DataModels;
using Api.Features.Templates.Queries;
using Api.Generators.ECMAScript6;
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
            private readonly ECMAScript6Generator _ecmaScript6Generator;

            public Handler(CloudspoolContext db, ECMAScript6Generator ecmaScript6Generator)
            {
                _db = db;
                _ecmaScript6Generator = ecmaScript6Generator;
            }

            [HttpPost("/Templates")]
            [Consumes("application/javascript")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                using var reader = new StreamReader(Request.Body);
                var script = await reader.ReadToEndAsync();

                var errors = _ecmaScript6Generator.ValidateTemplate(script);

                if (errors.Length > 0)
                {
                    return BadRequest(errors);
                }

                var template = new Template(projectId, request.Name, script, Request.ContentType);
                _db.Template.Add(template);
                await _db.SaveChangesAsync();

                return RedirectToEndpoint(new GetById.Query() { Id = template.Id});
            }
        }
    }
}
