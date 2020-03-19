using System.Collections.Generic;

namespace Api.Client.Models
{
    public class Zone
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public List<Route> Routes { get; set; }

        public class Route
        {
            public int Id { get; set; }
            public int ZoneId { get; set; }
            public string Alias { get; set; }
            public int SpoolerId { get; set; }
            public string PrinterName { get; set; }
        }
    }
}
