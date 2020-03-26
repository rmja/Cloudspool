using Refit;
using System.Threading.Tasks;

namespace Api.Client
{
    public partial interface IApiClient
    {
        [Post("/Documents/{documentId}/Print")]
        [Headers("Authorization: Bearer")]
        Task JobsPrintDocumentAsync(int documentId);
    }
}
