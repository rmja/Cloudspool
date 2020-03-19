using System;

namespace Api.DataModels
{
	public class Format
	{
		public int Id { get; set; }
		public int ZoneId { get; set; }
		public string Alias { get; set; }
		public int TemplateId { get; set; }
		public DateTime Created { get; set; }

		public Zone Zone { get; private set; }
		public Template Template { get; private set; }

		private Format()
		{
		}

		public Format(int zoneId, string alias, int templateId)
        {
			ZoneId = zoneId;
			TemplateId = templateId;
			Alias = alias;
			Created = DateTime.UtcNow;
		}
	}
}
