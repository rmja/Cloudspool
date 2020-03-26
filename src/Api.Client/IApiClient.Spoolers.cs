using Api.Client.Models;
using Refit;
using System;
using System.Threading.Tasks;

namespace Api.Client
{
    public partial interface IApiClient
    {
        [Get("/Spoolers/{spoolerKey}")]
        Task<Spooler> SpoolerGetByKeyAsync(Guid spoolerKey);

        [Put("/Spoolers/{spoolerId}/Printers")]
        [Headers("Authorization: Bearer")]
        Task SpoolerSetPrintersAsync(int spoolerId, [Body] string[] printerNames);
    }
}
