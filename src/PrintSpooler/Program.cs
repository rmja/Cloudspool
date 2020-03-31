using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintSpooler.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PrintSpooler
{
    public class Program
    {
        const string ServiceName = "CloudspoolPrintSpooler";

        public static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            if (args.Length > 0)
            {
                if (!args.Contains("--disable-update"))
                {
                    var appUpdater = host.Services.GetRequiredService<AppUpdater>();
                    var appLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                    await appUpdater.EnsureUpdatedAsync();

                    if (appLifetime.ApplicationStopping.IsCancellationRequested)
                    {
                        return 1;
                    }
                }

                switch (args[0])
                {
                    case "setup":
                        {
                            Console.WriteLine("Welcome to the Cloudspool Print Spooler Setup");

                            Console.Write("Enter spooler key: ");
                            var keyString = Console.ReadLine();

                            Guid key;
                            while (!Guid.TryParse(keyString, out key))
                            {
                                Console.Write("Invalid key, retry: ");
                                keyString = Console.ReadLine();
                            }

                            Console.WriteLine("Installing service...");

                            var dllPath = Assembly.GetEntryAssembly().Location;
                            var exePath = Path.ChangeExtension(dllPath, ".exe");

                            ServiceControl.Create(ServiceName, new Dictionary<string, string>()
                            {
                                ["binPath"] = $"{exePath} --service --spoolerkey={key}",
                                ["start"] = "auto",
                                ["displayName"] = "Cloudspool Print Spooler"
                            });

                            Console.Write("Start service now? [Y/n] ");

                            var answer = Console.ReadLine();

                            if (answer == string.Empty || answer.Equals("y", StringComparison.OrdinalIgnoreCase))
                            {
                                ServiceControl.Start(ServiceName);

                                Console.WriteLine("Service was started");
                            }

                            Console.WriteLine("All done!");
                            return 0;
                        }
                    case "install":
                        {
                            var dllPath = Assembly.GetEntryAssembly().Location;
                            var exePath = Path.ChangeExtension(dllPath, ".exe");

                            ServiceControl.Create(ServiceName, new Dictionary<string, string>()
                            {
                                ["binPath"] = $"{exePath} --service",
                                ["start"] = "auto",
                                ["displayName"] = "Cloudspool Print Spooler"
                            });
                            return 0;
                        }
                    case "uninstall":
                        ServiceControl.Delete(ServiceName);
                        return 0;
                    case "start":
                        ServiceControl.Start(ServiceName);
                        return 0;
                    case "stop":
                        ServiceControl.Stop(ServiceName);
                        return 0;
                    case "status":
                        {
                            var state = ServiceControl.GetState(ServiceName);

                            if (state == ServiceState.NotInstalled)
                            {
                                Console.WriteLine("Service is not installed, run 'PrintSpooler install' to install.");
                            }
                            else
                            {
                                Console.WriteLine($"Service is installed, current state is '{state}'.");
                            }
                            return 0;
                        }
                }
            }

            await host.RunAsync();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
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
