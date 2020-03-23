using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Templates.Queries
{
    public class GetById
    {
        public class Query
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Template>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Templates/{Id:int}", Name = RouteNames.GetTemplateById)]
            public override async Task<ActionResult<Template>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Template
                    .Where(x => x.ProjectId == projectId && x.Id == request.Id);
                var result = await _mapper.ProjectTo<Template>(query).SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                result.ScriptUrl = Url.RouteUrl(RouteNames.GetTemplateScriptById, new GetScriptById.Query() { Id = result.Id });

                return result;
            }
        }
    }
}
