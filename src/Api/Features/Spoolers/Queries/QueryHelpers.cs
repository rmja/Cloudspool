using Api.Client.Models;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Features.Spoolers.Queries
{
    public static class QueryHelpers
    {
        public static async Task GetPrintersAsync(Spooler spooler, IDatabase db)
        {
            var printers = await db.SetMembersAsync($"spoolers:{spooler.Id}:printers");
            spooler.Printers = printers.Select(x => (string)x).ToArray();
        }
    }
}
