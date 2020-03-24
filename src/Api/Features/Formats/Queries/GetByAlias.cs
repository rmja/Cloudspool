using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Formats.Queries
{
    public class GetByAlias
    {
        public const string RouteName = "GetFormatByAlias";

        public class Query
        {
            public int ZoneId { get; set; }
            public string Alias { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Format>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Zones/{ZoneId:int}/Formats/{Alias}", Name = RouteName)]
            public override async Task<ActionResult<Format>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Format
                    .Where(x => x.Zone.ProjectId == projectId && x.ZoneId == request.ZoneId && x.Alias == request.Alias);
                var result = await _mapper.ProjectTo<Format>(query).SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                return result;
            }
        }
    }
}
