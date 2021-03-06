﻿using Api.Features.Resources.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Resources.Commands
{
    public class Set
    {
        public class Command
        {
            public string Alias { get; set; }
        }

        public class Handler : ApiEndpoint<Command>
        {
            private readonly CloudspoolContext _db;

            public Handler(CloudspoolContext db)
            {
                _db = db;
            }

            [HttpPut("/Resources/{Alias}/Content")]
            public override async Task<ActionResult> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                var projectId = User.GetProjectId();

                using var content = new MemoryStream();
                await Request.Body.CopyToAsync(content);

                var resource = await _db.Resources.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Alias == request.Alias);

                var mediaType = Request.GetTypedHeaders().ContentType.MediaType.Value;
                if (resource is object)
                {
                    resource.Content = content.ToArray();
                    resource.MediaType = mediaType;
                }
                else
                {
                    resource = new DataModels.Resource(projectId, request.Alias, content.ToArray(), mediaType);
                    _db.Resources.Add(resource);
                }

                await _db.SaveChangesAsync();

                return SeeOtherEndpoint(new GetByAlias.Query() { Alias = request.Alias });
            }
        }
    }
}
