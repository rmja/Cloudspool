using System;

namespace Api.DataModels
{
	public class TerminalRoute
	{
		public int Id { get; set; }
		public int TerminalId { get; set; }
		public Terminal Terminal { get; set; }
		public string Alias { get; set; }
		public int SpoolerId { get; set; }
		public Spooler Spooler { get; set; }
		public string PrinterName { get; set; }
		public DateTime Created { get; set; }

		private TerminalRoute()
		{
		}

		public TerminalRoute(string alias)
		{
			Alias = alias;
			Created = DateTime.UtcNow;
		}
	}
}
