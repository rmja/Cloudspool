using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Terminals.Queries
{
    public class GetAllByZone
    {
        public class Query
        {
            public int ZoneId { get; set; }
        }

        public class Handler : ApiEndpoint<Query, List<Terminal>>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Zones/{ZoneId:int}/Terminals")]
            public override async Task<ActionResult<List<Terminal>>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Terminals
                    .Where(x => x.Zone.ProjectId == projectId && x.ZoneId == request.ZoneId);
                var result = await _mapper.ProjectTo<Terminal>(query).ToListAsync();

                return result;
            }
        }
    }
}
