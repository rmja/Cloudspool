using System;
using System.Collections.Generic;

namespace Api.DataModels
{
	public class Terminal
	{
		public int Id { get; set; }
		public int ZoneId { get; set; }
		public string Name { get; set; }
		public Guid Key { get; set; }
		public DateTime Created { get; set; }
		public List<TerminalRoute> Routes { get; private set; } = new List<TerminalRoute>();

		public Zone Zone { get; set; }

		private Terminal()
		{
		}

		public Terminal(int zoneId, string name)
		{
			ZoneId = zoneId;
			Name = name;
			Key = Guid.NewGuid();
			Created = DateTime.UtcNow;
		}

		public void AddRoute(string alias, int spoolerId, string printerName)
		{
			Routes.Add(new TerminalRoute(Routes.Count, alias, spoolerId, printerName));
		}
	}
}
