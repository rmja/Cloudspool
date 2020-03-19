using System;

namespace Api.DataModels
{
    public class Template
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string Name { get; set; }
        public string Script { get; set; }
        public string ScriptContentType { get; set; }
        public DateTime Created { get; set; }

        private Template()
        {
        }

        public Template(int projectId, string name, string script, string scriptContentType)
        {
            ProjectId = projectId;
            Name = name;
            Script = script;
            ScriptContentType = scriptContentType;
            Created = DateTime.UtcNow;
        }
    }
}
