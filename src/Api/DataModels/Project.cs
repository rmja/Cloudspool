using System;

namespace Api.DataModels
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid Key { get; set; }
        public DateTime Created { get; set; }
    }
}
