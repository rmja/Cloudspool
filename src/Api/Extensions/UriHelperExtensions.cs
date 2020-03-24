using Microsoft.AspNetCore.Mvc;

namespace Api
{
    public static class UriHelperExtensions
    {
        public static string EndpointUrl<TEndpointQuery>(this IUrlHelper url, TEndpointQuery query)
        {
            var routeName = ApiEndpointInfo<TEndpointQuery>.RouteName;

            return url.RouteUrl(routeName, query);
        }
    }
}
