using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Api
{
    [ApiController]
    public abstract class ApiEndpoint<TRequest> : ControllerBase
    {
        public abstract Task<ActionResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }

    [ApiController]
    public abstract class ApiEndpoint<TRequest, TResponse> : ControllerBase
    {
        public abstract Task<ActionResult<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
