using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Queries
{
    public class GetByKey
    {
        public class Query
        {
            public Guid Key { get; set; }
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

            [AllowAnonymous]
            [HttpGet("/Spoolers/{key:guid}")]
            public override async Task<ActionResult<Spooler>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var query = _db.Spoolers.Where(x => x.Key == request.Key);
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
