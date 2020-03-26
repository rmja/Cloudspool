using Api.Client.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Documents.Queries
{
    public class GetById
    {
        public class Query
        {
            public int Id { get; set; }
        }

        public class Handler : ApiEndpoint<Query, Document>
        {
            private readonly CloudspoolContext _db;
            private readonly IMapper _mapper;

            public Handler(CloudspoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            [HttpGet("/Documents/{Id:int}", Name = "GetDocumentById")]
            public override async Task<ActionResult<Document>> HandleAsync(Query request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                var query = _db.Documents
                    .Where(x => x.ProjectId == projectId && x.Id == request.Id);
                var result = await _mapper.ProjectTo<Document>(query).SingleOrDefaultAsync();

                if (result is null)
                {
                    return NotFound();
                }

                result.ContentUrl = Url.EndpointUrl(new GetContentById.Query() { Id = result.Id });

                return result;
            }
        }
    }
}
