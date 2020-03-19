using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Tests
{
    public static class HttpContentExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<TValue> ReadAsJsonAsync<TValue>(this HttpContent content)
        {
            using var stream = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TValue>(stream, _jsonSerializerOptions);
        }
    }
}
