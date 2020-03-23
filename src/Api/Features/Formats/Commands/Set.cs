using Api.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Formats.Commands
{
    public class Set
    {
        public class Command
        {
            public int ZoneId { get; set; }
            public string Alias { get; set; }
            [FromBody]
            public Body Body { get; set; }
        }

        public class Body
        {
            [Required]
            public int TemplateId { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpPut("/Zones/{ZoneId:int}/Formats/{Alias}")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var zone = await _db.Zone.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == request.ZoneId);

                if (zone is null)
                {
                    return NotFound();
                }

                var format = new Format(zone.Id, request.Alias, request.Body.TemplateId);
                _db.Format.Add(format);
                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
