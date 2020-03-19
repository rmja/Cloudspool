using System;

namespace Api.DataModels
{
	public class Document
	{
		public int Id { get; set; }
		public int ProjectId { get; set; }
		public int? TemplateId { get; set; }
		public byte[] Content { get; set; }
		public string ContentType { get; set; }
		public DateTime Created { get; set; }

		public Document(int projectId, byte[] content, string contentType)
        {
			ProjectId = projectId;
			Content = content;
			ContentType = contentType;
			Created = DateTime.UtcNow;
		}

		public Document(int projectId, int templateId, byte[] content, string contentType)
        {
			ProjectId = projectId;
			TemplateId = templateId;
			Content = content;
			ContentType = contentType;
			Created = DateTime.UtcNow;
		}
	}
}
