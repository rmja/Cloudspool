using System;

namespace PrintSpooler
{
    public class PrintSpoolerOptions
    {
        public string ServiceName { get; set; } = Constants.DefaultServiceName;
        public string PrintingHubUrl { get; set; }
        public Guid SpoolerKey { get; set; }
        public string LocalPackageRepository { get; set; }
        public string UpdateeName { get; set; }
        public bool DisableUpdates { get; set; }
    }
}
