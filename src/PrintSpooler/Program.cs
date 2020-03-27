using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintSpooler.Proxy;
using System.Threading.Tasks;

namespace PrintSpooler
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var appUpdater = host.Services.GetRequiredService<AppUpdater>();
            await appUpdater.EnsureUpdatedAsync();

            await host.RunAsync();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<PrintingHubProxy>();
                    services.AddTransient<AppUpdater>();
                    services.Configure<PrintSpoolerOptions>(hostContext.Configuration);
                })
                .UseWindowsService();
    }
}
