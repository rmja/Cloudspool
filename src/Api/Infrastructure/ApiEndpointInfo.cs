using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;
using System.Reflection;

namespace Api
{
    public static class ApiEndpointInfo<TEndpointRequest>
    {
        public static string RouteName { get; } = GetRouteName(typeof(TEndpointRequest));

        private static string GetRouteName(Type requestType)
        {
            var assembly = requestType.Assembly;
            var handlerType = assembly.DefinedTypes.SingleOrDefault(type => IsHandlerFor(type, requestType));
            if (handlerType is null)
            {
                throw new InvalidOperationException($"Handler could not be found for request type {requestType}");
            }

            var httpMethodAttribute = handlerType.GetMethods().Select(x => x.GetCustomAttribute<HttpMethodAttribute>()).SingleOrDefault(x => x is object);
            if (httpMethodAttribute is null)
            {
                throw new InvalidOperationException($"Handler {handlerType} does not have a route");
            }
            if (httpMethodAttribute.Name is null)
            {
                throw new InvalidOperationException($"The route on {handlerType} does not have an explicit name");
            }

            return httpMethodAttribute.Name;
        }

        private static bool IsHandlerFor(Type type, Type requestType)
        {
            if (!type.IsAbstract && type.BaseType.IsGenericType)
            {
                var genericBaseType = type.BaseType.GetGenericTypeDefinition();

                if (genericBaseType == typeof(ApiEndpoint<>) || genericBaseType == typeof(ApiEndpoint<,>))
                {
                    var handlerRequestType = type.BaseType.GetGenericArguments()[0];

                    return handlerRequestType == requestType;
                }
            }

            return false;
        }
    }
}
