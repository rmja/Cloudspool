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
        public string ScriptMediaType { get; set; }
        public DateTime Created { get; set; }

        private Template()
        {
        }

        public Template(int projectId, string name, string script, string scriptMediaType)
        {
            ProjectId = projectId;
            Name = name;
            Script = script;
            ScriptMediaType = scriptMediaType;
            Created = DateTime.UtcNow;
        }
    }
}
