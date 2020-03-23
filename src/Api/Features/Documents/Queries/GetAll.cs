using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Documents.Queries
{
    public class GetAll
    {
        public class Query
        {
        }

        public class Handler : ApiEndpoint<Query, List<Document>>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Documents")]
            public override async Task<ActionResult<List<Document>>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Document
                    .Where(x => x.ProjectId == projectId);
                var documents = await _mapper.ProjectTo<Document>(query).ToListAsync();

                return documents;
            }
        }
    }
}
