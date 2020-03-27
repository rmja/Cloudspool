using System;

namespace Cloudspool.Api.Client.Models
{
    public class Spooler
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int ZoneId { get; set; }
        public string Name { get; set; }
        public Guid Key { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastConnected { get; set; }
        public DateTime? LastDisconnected { get; set; }
        public DateTime? LastHelloReceived { get; set; }
        public DateTime? LastHeartbeatReceived { get; set; }
        public DateTime? LastJobSpooled { get; set; }
        public string PrintSpoolerAppVersion { get; set; }
        public string[] Printers { get; set; }
    }
}
