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
        public static string PrintJobQueue(int spoolerId) => $"cloudspool:spoolers:{spoolerId}:print-job-queue";
        public static string GetInstalledPrintersQueue(int spoolerId) => $"cloudspool:spoolers:{spoolerId}:get-installed-printers-queue";
    }
}