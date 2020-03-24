using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Api
{
    [ApiController]
    public abstract class ApiEndpoint<TRequest> : ControllerBase
    {
        private static readonly ConcurrentDictionary<Type, string> _routeNames = new ConcurrentDictionary<Type, string>();

        public abstract Task<ActionResult> HandleAsync(TRequest request, CancellationToken cancellationToken);

        [NonAction]
        public virtual ActionResult RedirectToEndpoint<TEndpointQuery>(TEndpointQuery query)
        {
            var routeName = _routeNames.GetOrAdd(query.GetType(), ComputeRouteName);

            return RedirectToRoute(routeName, query);
        }

        [NonAction]
        public virtual ActionResult SeeOtherEndpoint<TEndpointQuery>(TEndpointQuery query)
        {
            var routeName = _routeNames.GetOrAdd(query.GetType(), ComputeRouteName);

            return SeeOtherRoute(routeName, query);
        }

        [NonAction]
        public virtual ActionResult SeeOtherRoute(string routeName, object routeValues)
        {
            Response.Headers[HeaderNames.Location] = Url.RouteUrl(routeName, routeValues);
            return StatusCode(StatusCodes.Status303SeeOther);
        }

        private static string ComputeRouteName(Type queryType)
        {
            var routeNameField = queryType.DeclaringType.GetField("RouteName");

            if (routeNameField is null || !routeNameField.IsLiteral || routeNameField.FieldType != typeof(string))
            {
                throw new InvalidOperationException("const RouteName string field not found");
            }

            return (string)routeNameField.GetValue(null);
        }
    }

    [ApiController]
    public abstract class ApiEndpoint<TRequest, TResponse> : ControllerBase
    {
        public abstract Task<ActionResult<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
