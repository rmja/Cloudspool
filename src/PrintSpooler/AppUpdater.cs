using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Onova;
using Onova.Models;
using Onova.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PrintSpooler
{
    class AppUpdater : BackgroundService
    {
        private const int ERROR_SUCCESS_REBOOT_INITIATED = 1641; // https://stackoverflow.com/questions/220382/how-can-a-windows-service-programmatically-restart-itself#comment73265562_220451

        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly PrintSpoolerOptions _options;
        private readonly ILogger<AppUpdater> _logger;
        private readonly UpdateManager _um;

        public AppUpdater(IHostApplicationLifetime applicationLifetime, IOptions<PrintSpoolerOptions> options, ILogger<AppUpdater> logger)
        {
            _applicationLifetime = applicationLifetime;
            _options = options.Value;
            _logger = logger;

            IPackageResolver resolver;

            if (_options.LocalPackageRepository is object)
            {
                resolver = new LocalPackageResolver(_options.LocalPackageRepository);
            }
            else
            {
                resolver = new GithubPackageResolver("rmja", "Cloudspool", "PrintSpooler-win64.zip");
            }

            var entryAssembly = AssemblyMetadata.FromEntryAssembly();

            var updateeName = _options.UpdateeName ?? entryAssembly.Name;
            var updatee = new AssemblyMetadata(updateeName, entryAssembly.Version, entryAssembly.FilePath);

            _um = new UpdateManager(
                updatee,
                resolver,
                new ZipPackageExtractor());
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Updatee} {Version}", _um.Updatee.Name, _um.Updatee.Version.ToString(3));

            if (!_options.DisableUpdates)
            {
                var preparedUpdates = _um.GetPreparedUpdates();

                if (preparedUpdates.Count > 0)
                {
                    var newestPreparedUpdateVersion = preparedUpdates.Max();

                    if (newestPreparedUpdateVersion > _um.Updatee.Version)
                    {
                        _um.LaunchUpdater(newestPreparedUpdateVersion, restart: false);

                        if (Program.IsWindowsService)
                        {
                            _logger.LogInformation("Update {Updatee} to version {PreparedVersion} completed, auto-restarting service in 5 seconds...", _um.Updatee.Name, newestPreparedUpdateVersion.ToString(3));

                            Environment.ExitCode = ERROR_SUCCESS_REBOOT_INITIATED; // Windows does not restart the service if we return with exit code 0

                            _applicationLifetime.StopApplication();
                        }
                        else
                        {
                            _logger.LogInformation("Update {Updatee} to version {PreparedVersion} completed, shutting down...", _um.Updatee.Name, newestPreparedUpdateVersion.ToString(3));
                        }
                    }
                }
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_options.DisableUpdates)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var check = await _um.CheckForUpdatesAsync();

                    if (check.CanUpdate)
                    {
                        _logger.LogInformation("Downloading {UpdateeName} version {NewVersion}", _um.Updatee.Name, check.LastVersion.ToString(3));

                        await _um.PrepareUpdateAsync(check.LastVersion);

                        _logger.LogInformation("Download preparation completed");
                    }
                    else
                    {
                        _logger.LogInformation("There are no updates available");
                    }
                }
                catch (HttpRequestException e) when (e.Message.Contains("rate limit exceeded", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Unable to check for update because of GitHub rate limiting");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        public override void Dispose()
        {
            _um.Dispose();
            base.Dispose();
        }
    }
}
