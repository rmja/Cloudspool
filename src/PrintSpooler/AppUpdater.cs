using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Onova;
using Onova.Models;
using Onova.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrintSpooler
{
    class AppUpdater
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<AppUpdater> _logger;

        public AppUpdater(IHostApplicationLifetime applicationLifetime, ILogger<AppUpdater> logger)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }

        public async Task EnsureUpdatedAsync()
        {
            var updatee = AssemblyMetadata.FromEntryAssembly();

            _logger.LogInformation("PrintSpooler {Version}", updatee.Version);

            var storageDirPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Onova", updatee.Name);

            // Set lock file path
            var lockFilePath = Path.Combine(storageDirPath, "Onova.lock");

            _logger.LogInformation("Lock file is at {LockFilePath}", lockFilePath);

            using (var um = new UpdateManager(
                updatee,
                new GithubPackageResolver("rmja", "Cloudspool", "PrintSpooler-win64.zip"),
                new ZipPackageExtractor()))
            {
                try
                {
                    var check = await um.CheckForUpdatesAsync();

                    if (!check.CanUpdate)
                    {
                        _logger.LogInformation("There are no updates available");
                    }
                    else
                    {
                        _logger.LogInformation("Downloading version {NewVersion}", check.LastVersion);

                        await um.PrepareUpdateAsync(check.LastVersion);

                        _logger.LogInformation("Launching updater");

                        um.LaunchUpdater(check.LastVersion);

                        _logger.LogInformation("Updater completed");

                        _applicationLifetime.StopApplication();
                    }
                }
                catch (HttpRequestException e) when (e.Message.Contains("rate limit exceeded", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Unable to check for update because of GitHub rate limiting");
                }
            }
        }
    }
}
