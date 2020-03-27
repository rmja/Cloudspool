using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintSpooler.Proxy;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PrintSpooler
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            if (!args.Contains("--disable-updates"))
            {
                var appUpdater = host.Services.GetRequiredService<AppUpdater>();
                var appLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                await appUpdater.EnsureUpdatedAsync();

                if (appLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    return 1;
                }
            }

            await host.RunAsync();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<PrintWorker>();
                    services.AddHostedService<HeartbeatWorker>();
                    services.AddSingleton<PrintingHubProxy>();
                    services.AddTransient<AppUpdater>();
                    services.Configure<PrintSpoolerOptions>(hostContext.Configuration);
                })
                .UseWindowsService();
    }
}
