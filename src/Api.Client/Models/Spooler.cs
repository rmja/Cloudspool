using System;

namespace Api.Client.Models
{
    public class Spooler
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int ZoneId { get; set; }
        public string Name { get; set; }
        public Guid Key { get; set; }
        public string[] Printers { get; set; }
    }
}
