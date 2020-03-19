using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Tests
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PatchAsJsonAsync(this HttpClient client, string requestUri, IEnumerable<Operation> operations)
        {
            var content = new PushStreamContent(async (stream, content, context) =>
            {
                await using (stream)
                {
                    await JsonSerializer.SerializeAsync(stream, operations);
                    await stream.FlushAsync();
                }
            }, "application/json-patch+json");

            return client.PatchAsync(requestUri, content);
        }
    }
}
