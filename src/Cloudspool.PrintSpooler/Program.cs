using Cloudspool.PrintSpooler.Proxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Onova;
using Onova.Models;
using Onova.Services;
using System;
using System.Threading.Tasks;

namespace Cloudspool.PrintSpooler
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var updatee = AssemblyMetadata.FromEntryAssembly();

            Console.WriteLine($"PrintSpooler v{updatee.Version}");

            using (var um = new UpdateManager(
                updatee,
                new GithubPackageResolver("rmja", "Cloudspool", "PrintSpooler-*.zip"),
                new ZipPackageExtractor()))
            {
                var check = await um.CheckForUpdatesAsync();

                if (!check.CanUpdate)
                {
                    Console.WriteLine("There are no updates available");
                }
                else
                {
                    Console.WriteLine($"Preparing update to v{check.LastVersion}");

                    await um.PrepareUpdateAsync(check.LastVersion);

                    um.LaunchUpdater(check.LastVersion);
                    return 1;
                }
            }

            await CreateHostBuilder(args).Build().RunAsync();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<PrintingHubProxy>();
                    services.Configure<PrintSpoolerOptions>(hostContext.Configuration);
                })
                .UseWindowsService();
    }
}
