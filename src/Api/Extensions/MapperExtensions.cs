using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;

namespace Api
{
    public static class MapperExtensions
    {
        public static T Patch<T>(this IMapper mapper, object source, JsonPatchDocument<T> patch) where T : class
        {
            var patched = mapper.Map<T>(source);
            patch.ApplyTo(patched);
            return patched;
        }
    }
}
