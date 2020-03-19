using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Queries
{
    public class GetAll
    {
        public class Query
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Query, List<Spooler>>
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

            [HttpGet("/Spoolers")]
            public override async Task<ActionResult<List<Spooler>>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Spoolers.Where(x => x.Zone.ProjectId == projectId);
                var spoolers = await _mapper.ProjectTo<Spooler>(query).ToListAsync();

                if (spoolers is null)
                {
                    return NotFound();
                }

                var db = _redis.GetDatabase();
                await Task.WhenAll(spoolers.Select(x => QueryHelpers.GetPrintersAsync(x, db)));

                return spoolers;
            }
        }
    }
}
