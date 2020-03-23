using Api.DataModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Features.Terminals.Commands
{
    public static class CommandHelpers
    {
        public static async Task<bool> HasValidRoutes(Terminal terminal, CloudspoolContext db, int projectId)
        {
            var ids = new HashSet<int>(terminal.Routes.Select(x => x.SpoolerId));

            return await db.Spooler.Where(x => x.Zone.ProjectId == projectId).CountAsync(x => ids.Contains(x.Id)) == ids.Count;
        }
    }
}
