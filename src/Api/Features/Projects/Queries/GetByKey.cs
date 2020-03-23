using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Projects.Queries
{
    public class GetByKey
    {
        public class Query
        {
            public Guid Key { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Project>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [AllowAnonymous]
            [HttpGet("/Projects/{Key:guid}")]
            public override async Task<ActionResult<Project>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var query = _db.Projects.Where(x => x.Key == request.Key);
                var project = await _mapper.ProjectTo<Project>(query).SingleOrDefaultAsync();

                if (project is null)
                {
                    return NotFound();
                }

                return project;
            }
        }
    }
}
