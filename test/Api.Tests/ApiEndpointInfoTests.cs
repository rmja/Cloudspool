using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests
{
    public class ApiEndpointInfoTests
    {
        [Fact]
        public void CanGetRouteNameFromQuery()
        {
            Assert.Equal("CreateCommand", ApiEndpointInfo<CommandRequest>.RouteName);
            Assert.Equal("GetAllCommands", ApiEndpointInfo<QueryRequest>.RouteName);
        }

        public class CommandRequest
        {
        }

        public class CommandHandler : ApiEndpoint<CommandRequest>
        {
            [HttpPost("/Commands", Name = "CreateCommand")]
            public override Task<ActionResult> HandleAsync(CommandRequest request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public class QueryRequest
        {

        }

        public class QueryResponse
        {
        }

        public class QueryHandler : ApiEndpoint<QueryRequest, QueryResponse>
        {
            [HttpGet("/Commands", Name = "GetAllCommands")]
            public override Task<ActionResult<QueryResponse>> HandleAsync(QueryRequest request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
