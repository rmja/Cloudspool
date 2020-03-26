using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Resources.Queries
{
    public class GetAll
    {
        public class Query
        {
        }

        public class Handler : ApiEndpoint<Query, List<Resource>>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Resources")]
            public override async Task<ActionResult<List<Resource>>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Resources.Where(x => x.ProjectId == projectId);
                var resources = await _mapper.ProjectTo<Resource>(query).ToListAsync();

                foreach (var resource in resources)
                {
                    resource.ContentUrl = Url.EndpointUrl(new GetContentByAlias.Query() { Alias = resource.Alias });
                }

                return resources;
            }
        }
    }
}
