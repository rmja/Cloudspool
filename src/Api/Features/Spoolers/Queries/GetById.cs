using Cloudspool.Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Intercom;
using System;

namespace Api.Features.Spoolers.Queries
{
    public class GetById
    {
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

            [HttpGet("/Spoolers/{Id:int}", Name = "GetSpoolerById")]
            public override async Task<ActionResult<Spooler>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Spoolers.Where(x => x.Zone.ProjectId == projectId && x.Id == request.Id);
                var spooler = await _mapper.ProjectTo<Spooler>(query).SingleOrDefaultAsync();

                if (spooler is null)
                {
                    return NotFound();
                }

                var db = _redis.GetDatabase();

                if (Request.GetTypedHeaders().CacheControl?.NoCache == true)
                {
                    // Force update of installed printers
                    var subscriber = _redis.GetSubscriber();

                    var refreshedSubscription = await subscriber.SubscribeAsync(RedisConstants.Channels.InstalledPrintersRefreshed(spooler.Id));

                    var queue = RedisConstants.Queues.RequestInstalledPrintersRefreshQueue(spooler.Id);
                    var queueRequest = new RequestInstalledPrintersRefreshRequest()
                    {
                        SpoolerId = spooler.Id
                    };
                    await db.ListRightPushAsync(queue, JsonSerializer.Serialize(queueRequest));
                    await subscriber.PublishAsync(RedisConstants.Channels.JobCreated, queue);

                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        cts.CancelAfter(5_000);

                        await refreshedSubscription.ReadAsync(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    finally
                    {
                        await refreshedSubscription.UnsubscribeAsync();
                    }
                }
                
                await QueryHelpers.GetPrintersAsync(spooler, db);

                return spooler;
            }
        }
    }
}
