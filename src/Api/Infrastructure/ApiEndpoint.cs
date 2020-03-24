using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Api
{
    [ApiController]
    public abstract class ApiEndpoint<TRequest> : ControllerBase
    {
        public abstract Task<ActionResult> HandleAsync(TRequest request, CancellationToken cancellationToken);

        [NonAction]
        public virtual ActionResult RedirectToEndpoint<TEndpointQuery>(TEndpointQuery query)
        {
            var routeName = ApiEndpointInfo<TEndpointQuery>.RouteName;

            return RedirectToRoute(routeName, query);
        }

        [NonAction]
        public virtual ActionResult SeeOtherEndpoint<TEndpointQuery>(TEndpointQuery query)
        {
            var routeName = ApiEndpointInfo<TEndpointQuery>.RouteName;

            return SeeOtherRoute(routeName, query);
        }

        [NonAction]
        public virtual ActionResult SeeOtherRoute(string routeName, object routeValues)
        {
            Response.Headers[HeaderNames.Location] = Url.RouteUrl(routeName, routeValues);
            return StatusCode(StatusCodes.Status303SeeOther);
        }
    }

    [ApiController]
    public abstract class ApiEndpoint<TRequest, TResponse> : ControllerBase
    {
        public abstract Task<ActionResult<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
