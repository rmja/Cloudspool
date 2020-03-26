using Api.Client.Models;
using Refit;
using System.Threading.Tasks;

namespace Api.Client
{
    public partial interface IApiClient
    {
        [Post("/Documents/Generate")]
        [Headers("Authorization: Bearer")]
        Task<Document> DocumentsGenerateAsync(string format, [Body] object model);
    }
}
