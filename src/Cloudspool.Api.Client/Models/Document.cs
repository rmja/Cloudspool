namespace Cloudspool.Api.Client.Models
{
    public class Document
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int TerminalId { get; set; }
        public int? TemplateId { get; set; }
        public string ContentType { get; set; }
        public string ContentUrl { get; set; }
	}
}
