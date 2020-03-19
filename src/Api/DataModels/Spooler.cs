using System;

namespace Api.DataModels
{
	public class Spooler
	{
		public int Id { get; set; }
		public int ZoneId { get; set; }
		public string Name { get; set; }
		public Guid Key { get; set; }
		public DateTime Created { get; set; }

		public Zone Zone { get; set; }

		private Spooler()
		{
		}

		public Spooler(int zoneId, string name)
		{
			ZoneId = zoneId;
			Name = name;
			Key = Guid.NewGuid();
			Created = DateTime.UtcNow;
		}
	}
}
