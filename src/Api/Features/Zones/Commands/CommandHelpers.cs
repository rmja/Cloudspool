using Api.DataModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Features.Zones.Commands
{
    public static class CommandHelpers
    {
        public static async Task<bool> HasValidRoutes(Zone zone, CloudspoolContext db, int projectId)
        {
            var ids = new HashSet<int>(zone.Routes.Select(x => x.SpoolerId));

            return await db.Spoolers.Where(x => x.Zone.ProjectId == projectId).CountAsync(x => ids.Contains(x.Id)) == ids.Count;
        }
    }
}
