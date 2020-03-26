public static class RedisConstants
{
    public const string ConnectedClients = "cloudspool:connected-clients";
    public static string InstalledPrinters(int spoolerId) => $"cloudspool:spoolers:{spoolerId}:installed-printers";

    public static class Channels
    {
        public static string JobCreated { get; set; } = "cloudspool:job-created";
        public static string InstalledPrintersRefreshed(int spoolerId) => $"cloudspool:spooolers:{spoolerId}:installed-printers-refreshed";
    }

    public static class Queues
    {
        public const string PrintJobQueueSuffix = "print-job-queue";
        public static string PrintJobQueue(int spoolerId) => $"cloudspool:spoolers:{spoolerId}:{PrintJobQueueSuffix}";
        
        public const string RequestInstalledPrintersRefreshQueueSuffix = "request-installed-printers-refresh-queue";
        public static string RequestInstalledPrintersRefreshQueue(int spoolerId) => $"cloudspool:spoolers:{spoolerId}:{RequestInstalledPrintersRefreshQueueSuffix}";
    }
}