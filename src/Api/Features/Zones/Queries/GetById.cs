using Cloudspool.Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Zones.Queries
{
    public class GetById
    {
        public class Query
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Zone>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Zones/{Id:int}", Name = "GetZoneById")]
            public override async Task<ActionResult<Zone>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Zones
                    .Where(x => x.ProjectId == projectId && x.Id == request.Id);
                var result = await _mapper.ProjectTo<Zone>(query).SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                return result;
            }
        }
    }
}
