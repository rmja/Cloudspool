using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Client.Models
{
    public class Zone
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public Dictionary<string, Route> Routes { get; set; } = new Dictionary<string, Route>();
        internal List<Route> RoutesList { get => throw new InvalidOperationException(); set => Routes = value.ToDictionary(x => x.Alias, x => x); }

        public class Route
        {
            public int ZoneId { get; set; }
            public string Alias { get; set; }
            public int SpoolerId { get; set; }
            public string PrinterName { get; set; }
        }
    }
}
