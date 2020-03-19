using System;

namespace Api.DataModels
{
	public class TerminalRoute
	{
		public int Id { get; set; }
		public int TerminalId { get; set; }
		public Terminal Terminal { get; set; }
		public int Index { get; set; }
		public string Alias { get; set; }
		public int SpoolerId { get; set; }
		public Spooler Spooler { get; set; }
		public string PrinterName { get; set; }
		public DateTime Created { get; set; }

		private TerminalRoute()
		{
		}

		public TerminalRoute(int index, string alias, int spoolerId, string printerName)
		{
			Index = index;
			Alias = alias;
			SpoolerId = spoolerId;
			PrinterName = printerName;
			Created = DateTime.UtcNow;
		}
	}
}
