using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Reflection;

namespace Api.Infrastructure
{
    public class ScanNestedControllersFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly Assembly[] _assemblies;

        public ScanNestedControllersFeatureProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var assembly in _assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    if (type.IsNested && type.GetCustomAttribute<ApiControllerAttribute>(true) is object)
                    {
                        var typeInfo = type.GetTypeInfo();
                        if (!feature.Controllers.Contains(typeInfo))
                        {
                            feature.Controllers.Add(typeInfo);
                        }
                    }
                }
            }
        }
    }
}
