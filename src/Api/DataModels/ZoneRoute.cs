using System;

namespace Api.DataModels
{
	public class ZoneRoute
	{
		public int Id { get; set; }
		public int ZoneId { get; set; }
		public Zone Zone { get; set; }
		public string Alias { get; set; }
		public int SpoolerId { get; set; }
		public Spooler Spooler { get; set; }
		public string PrinterName { get; set; }
		public DateTime Created { get; set; }

		private ZoneRoute()
		{
		}

		public ZoneRoute(string alias)
		{
			Alias = alias;
			Created = DateTime.UtcNow;
		}
	}
}
