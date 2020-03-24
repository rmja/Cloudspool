using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Tests
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsJsonAsync(this HttpClient client, string requestUri, IEnumerable<Operation> operations)
        {
            var content = new PushStreamContent(async (stream, content, context) =>
            {
                await using (stream)
                {
                    await JsonSerializer.SerializeAsync(stream, operations);
                    await stream.FlushAsync();
                }
            }, "application/json-patch+json");

            var response = await client.PatchAsync(requestUri, content);

            if (response.StatusCode == HttpStatusCode.SeeOther)
            {
                response = await client.GetAsync(response.Headers.Location);
            }

            return response;
        }
    }
}
