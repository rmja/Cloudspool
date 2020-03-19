using System;

namespace Api.DataModels
{
	public class ZoneRoute
	{
		public int Id { get; set; }
		public int ZoneId { get; set; }
		public Zone Zone { get; set; }
		public int Index { get; set; }
		public string Alias { get; set; }
		public int SpoolerId { get; set; }
		public Spooler Spooler { get; set; }
		public string PrinterName { get; set; }
		public DateTime Created { get; set; }

		private ZoneRoute()
		{
		}

		public ZoneRoute(int index, string alias, int spoolerId, string printerName)
		{
			Index = index;
			Alias = alias;
			SpoolerId = spoolerId;
			PrinterName = printerName;
			Created = DateTime.UtcNow;
		}
	}
}
