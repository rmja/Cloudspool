using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PrintSpooler.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PrintSpooler
{
    public class Program
    {
        public static bool IsWindowsService { get; private set; }
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            IsWindowsService = args.Contains("--service");

            if (args.Length > 0)
            {
                var serviceName = host.Services.GetRequiredService<IOptions<PrintSpoolerOptions>>().Value.ServiceName;

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

                            ServiceControl.Create(serviceName, new Dictionary<string, string>()
                            {
                                ["binPath"] = $"{exePath} --service --servicename={serviceName} --spoolerkey={key}",
                                ["start"] = "auto",
                                ["displayName"] = GetServiceDisplayName(serviceName)
                            });

                            // Restart always (after 5000ms) when nonzero exit code is returned (after app update)
                            // The delay needs to be there because we need to release the PrintSpooler.dll file for it to be writeable
                            ServiceControl.Failure(serviceName, new Dictionary<string, string>()
                            {
                                ["reset"] = "0",
                                ["actions"] = "restart/5000/restart/5000/restart/5000"
                            });

                            Console.Write("Start service now? [Y/n] ");

                            var answer = Console.ReadLine();

                            if (answer == string.Empty || answer.Equals("y", StringComparison.OrdinalIgnoreCase))
                            {
                                ServiceControl.Start(serviceName);

                                Console.WriteLine("Service was started");
                            }

                            Console.WriteLine("All done!");
                            return;
                        }
                    case "install":
                        {
                            var dllPath = Assembly.GetEntryAssembly().Location;
                            var exePath = Path.ChangeExtension(dllPath, ".exe");

                            ServiceControl.Create(serviceName, new Dictionary<string, string>()
                            {
                                ["binPath"] = $"{exePath} --service --servicename={serviceName}",
                                ["start"] = "auto",
                                ["displayName"] = GetServiceDisplayName(serviceName)
                            });

                            // Restart always (after 5000ms) when nonzero exit code is returned (after app update)
                            // The delay needs to be there because we need to release the PrintSpooler.dll file for it to be writeable
                            ServiceControl.Failure(serviceName, new Dictionary<string, string>()
                            {
                                ["reset"] = "0",
                                ["actions"] = "restart/5000/restart/5000/restart/5000"
                            });
                            return;
                        }
                    case "uninstall":
                        ServiceControl.Delete(serviceName);
                        return;
                    case "start":
                        ServiceControl.Start(serviceName);
                        return;
                    case "stop":
                        ServiceControl.Stop(serviceName);
                        return;
                    case "status":
                        {
                            var state = ServiceControl.GetState(serviceName);

                            if (state == ServiceState.NotInstalled)
                            {
                                Console.WriteLine("Service is not installed, run 'PrintSpooler install' to install.");
                            }
                            else
                            {
                                Console.WriteLine($"Service is installed, current state is '{state}'.");
                            }
                            return;
                        }
                    case "version":
                        Console.WriteLine(Assembly.GetEntryAssembly().GetName().Version.ToString(3));
                        return;
                }
            }

            EventLog.WriteEntry("PrintSpooler", "Invoking RunAsync", EventLogEntryType.Information);

            await host.RunAsync();

            EventLog.WriteEntry("PrintSpooler", $"Returning exit code {Environment.ExitCode}", EventLogEntryType.Information);
        }

        private static string GetServiceDisplayName(string serviceName)
        {
            var name = "Cloudspool Print Spooler";

            if (serviceName != Constants.DefaultServiceName)
            {
                name += $" ({serviceName})";
            }

            return name;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<AppUpdater>();
                    services.AddHostedService<PrintWorker>();
                    services.AddHostedService<HeartbeatWorker>();
                    services.AddSingleton<PrintingHubProxy>();
                    services.Configure<PrintSpoolerOptions>(hostContext.Configuration);
                })
                .UseWindowsService();
    }
}
