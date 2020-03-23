using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Terminals.Queries
{
    public class GetById
    {
        public class Query
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Terminal>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Terminals/{Id:int}", Name = RouteNames.GetTerminalById)]
            public override async Task<ActionResult<Terminal>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Terminal
                    .Where(x => x.Zone.ProjectId == projectId && x.Id == request.Id);
                var result = await _mapper.ProjectTo<Terminal>(query).SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                return result;
            }
        }
    }
}
