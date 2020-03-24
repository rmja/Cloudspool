using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Queries
{
    public class GetById
    {
        public const string RouteName = "GetSpoolerById";

        public class Query
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Spooler>
        {
            private readonly CloudspoolContext _db;
            private readonly ConnectionMultiplexer _redis;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, ConnectionMultiplexer redis, IMapper mapper)
            {
                _db = db;
                _redis = redis;
                _mapper = mapper;
            }

            [HttpGet("/Spoolers/{Id:int}", Name = RouteName)]
            public override async Task<ActionResult<Spooler>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Spooler.Where(x => x.Zone.ProjectId == projectId && x.Id == request.Id);
                var spooler = await _mapper.ProjectTo<Spooler>(query).SingleOrDefaultAsync();

                if (spooler is null)
                {
                    return NotFound();
                }

                var db = _redis.GetDatabase();
                await QueryHelpers.GetPrintersAsync(spooler, db);

                return spooler;
            }
        }
    }
}
