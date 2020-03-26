using Cloudspool.Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Zones.Queries
{
    public class GetAll
    {
        public class Query
        {
        }

        public class Handler : ApiEndpoint<Query, List<Zone>>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Zones")]
            public override async Task<ActionResult<List<Zone>>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Zones
                    .Where(x => x.ProjectId == projectId);
                var result = await _mapper.ProjectTo<Zone>(query).ToListAsync();

                return result;
            }
        }
    }
}
