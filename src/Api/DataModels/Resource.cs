using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DataModels
{
    public class Resource
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Alias { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public DateTime Created { get; set; }
        public Project Project { get; set; }
    }
}
