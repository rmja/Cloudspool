using Cloudspool.Api.Client.Models;
using Refit;
using System;
using System.Threading.Tasks;

namespace Cloudspool.Api.Client
{
    public interface IApiClient
    {
        [Post("/Documents/Generate")]
        [Headers("Authorization: Bearer")]
        Task<Document> DocumentsGenerateAsync(string format, [Body] object model);

        [Post("/Documents/{documentId}/Print")]
        [Headers("Authorization: Bearer")]
        Task JobsPrintDocumentAsync(int documentId, string route);

        [Get("/Spoolers/{spoolerKey}")]
        Task<Spooler> SpoolerGetByKeyAsync(Guid spoolerKey);

        [Put("/Spoolers/{spoolerId}/Printers")]
        [Headers("Authorization: Bearer")]
        Task SpoolerSetPrintersAsync(int spoolerId, [Body] string[] printerNames);
    }
}
