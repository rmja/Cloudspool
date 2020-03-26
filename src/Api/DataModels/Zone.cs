using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.DataModels
{
	public class Zone
	{
		public int Id { get; set; }
		public int ProjectId { get; set; }
		public Project Project { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public List<ZoneRoute> Routes { get; private set; } = new List<ZoneRoute>();

		public ICollection<Format> Formats { get; set; }
		public ICollection<Spooler> Spoolers { get; set; }
		public ICollection<Terminal> Terminals { get; set; }

		private Zone()
		{
		}

		public Zone(int projectId, string name)
		{
			ProjectId = projectId;
			Name = name;
			Created = DateTime.UtcNow;
		}

		public ZoneRoute GetOrAddRoute(string alias)
		{
			var route = Routes.SingleOrDefault(x => x.Alias == alias);

			if (route is null)
			{
				route = new ZoneRoute(alias);
				Routes.Add(route);
			}

			return route;
		}
	}
}
