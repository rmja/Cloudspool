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
        public string MediaType { get; set; }
        public DateTime Created { get; set; }

        private Resource()
        {
        }

        public Resource(int projectId, string alias, byte[] content, string mediaType)
        {
            ProjectId = projectId;
            Alias = alias;
            Content = content;
            MediaType = mediaType;
            Created = DateTime.UtcNow;
        }
    }
}
