using Cloudspool.Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Formats.Queries
{
    public class GetAllByZone
    {
        public class Query
        {
            public int ZoneId { get; set; }
        }

        public class Handler : ApiEndpoint<Query, List<Format>>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Zones/{ZoneId:int}/Formats")]
            public override async Task<ActionResult<List<Format>>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Formats
                    .Where(x => x.Zone.ProjectId == projectId && x.ZoneId == request.ZoneId);
                var result = await _mapper.ProjectTo<Format>(query).ToListAsync();

                return result;
            }
        }
    }
}
