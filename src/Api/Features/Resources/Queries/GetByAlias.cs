using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Resources.Queries
{
    public class GetByAlias
    {
        public class Query
        {
            public string Alias { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Resource>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Resources/{Alias}", Name = "GetResourceByAlias")]
            public override async Task<ActionResult<Resource>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Resource.Where(x => x.ProjectId == projectId && x.Alias == request.Alias);
                var resource = await _mapper.ProjectTo<Resource>(query).SingleOrDefaultAsync();

                if (resource is null)
                {
                    return NotFound();
                }

                resource.ContentUrl = Url.EndpointUrl(new GetContentByAlias.Query() { Alias = resource.Alias });

                return resource;
            }
        }
    }
}
