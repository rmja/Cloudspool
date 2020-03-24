using System;

namespace Api.DataModels
{
    public class Resource
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string Alias { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public DateTime Created { get; set; }

        private Resource()
        {
        }

        public Resource(int projectId, string alias, byte[] content, string contentType)
        {
            ProjectId = projectId;
            Alias = alias;
            Content = content;
            ContentType = contentType;
            Created = DateTime.UtcNow;
        }
    }
}
