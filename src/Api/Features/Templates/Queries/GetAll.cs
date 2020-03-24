using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Templates.Queries
{
    public class GetAll
    {
        public class Query
        {
        }

        public class Handler : ApiEndpoint<Query, List<Template>>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Templates")]
            public override async Task<ActionResult<List<Template>>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Template
                    .Where(x => x.ProjectId == projectId);
                var templates = await _mapper.ProjectTo<Template>(query).ToListAsync();

                foreach (var template in templates)
                {
                    template.ScriptUrl = Url.RouteUrl(RouteNames.GetTemplateScriptById, new GetScriptById.Query() { Id = template.Id });
                }

                return templates;
            }
        }
    }
}
