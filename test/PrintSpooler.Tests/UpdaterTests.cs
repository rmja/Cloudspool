using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace PrintSpooler.Tests
{
    public class UpdaterTests
    {
        const string ServiceName = "CloudspoolPrintSpoolerTest";
        private readonly ITestOutputHelper _output;

        public UpdaterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanUpdateService()
        {
            // Use "sc delete CloudspoolPrintSpoolerTest" to delete the service if the test fails

            var preparedUpdatesDirectory = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Onova", "PrintSpooler-test"));

            if (preparedUpdatesDirectory.Exists)
            {
                preparedUpdatesDirectory.Delete(recursive: true);
            }

            var slnDirectory = GetSolutionDirectoryPath();

            var srcDirectoryPath = Path.Combine(slnDirectory, "src", "PrintSpooler");
            var packageRepositoryPath = Path.Combine(slnDirectory, "test", "PrintSpooler.Tests", "bin", "repository");

            Directory.CreateDirectory(packageRepositoryPath);

            foreach (var version in new[] { "1.0.0", "2.0.0" })
            {
                var outDirectory = new DirectoryInfo(Path.Combine(slnDirectory, "test", "PrintSpooler.Tests", "bin", $"v{version}"));

                if (outDirectory.Exists)
                {
                    outDirectory.Delete(recursive: true);
                }
                
                PublishProject(srcDirectoryPath, outDirectory.FullName, version);

                var zipFilePath = Path.Combine(packageRepositoryPath, $"PrintSpooler-win64.v{version}.zip");
                File.Delete(zipFilePath);
                ZipFile.CreateFromDirectory(outDirectory.FullName, zipFilePath, CompressionLevel.Fastest, includeBaseDirectory: false);
            }

            var directoryPath = Path.Combine(slnDirectory, "test", "PrintSpooler.Tests", "bin", $"v1.0.0");
            var exePath = Path.Combine(directoryPath, "PrintSpooler.exe");

            var appsettingsContent = JsonSerializer.Serialize(new
            {
                LocalPackageRepository = packageRepositoryPath,
                UpdateeName = "PrintSpooler-test",
                ServiceName = "CloudspoolPrintSpoolerTest"
            });
            File.WriteAllText(Path.Combine(directoryPath, "appsettings.Production.json"), appsettingsContent);

            RunCommand("install");
            RunCommand("start");

            // The update will be prepared during this start

            Thread.Sleep(10_000);

            RunCommand("stop");

            Thread.Sleep(1_000);

            RunCommand("start");
            // The update will be performed during this start
            // The service will terminate with a non-zero exit code when the updater is launched
            // The service is automatically restarted with a 5s delay making room for the updater to apply the update,
            // as it needs write access to the files that must be overwritten
            
            Thread.Sleep(15_000);

            RunCommand("stop");

            var updatedVersion = GetVersion();           

            RunCommand("uninstall");

            Assert.Equal("2.0.0", updatedVersion);



            void RunCommand(string command)
            {
                using var process = Process.Start(exePath, $"{command} --servicename={ServiceName}");
                process.WaitForExit();
                Assert.Equal(0, process.ExitCode);
            }

            string GetVersion()
            {
                using var process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.Arguments = "version";
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
                Assert.Equal(0, process.ExitCode);
                return process.StandardOutput.ReadLine();
            }
        }

        private void PublishProject(string projectPath, string outputDirectoryPath, string version)
        {
            using var p = new Process();
            p.StartInfo.FileName = "dotnet";
            p.StartInfo.Arguments = $"publish \"{projectPath}\" --output \"{outputDirectoryPath}\" --self-contained -r win-x64 /p:Version={version}";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.Start();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string line;

                while ((line = p.StandardOutput.ReadLine()) != null)
                {
                    _output.WriteLine(line);
                }

                while ((line = p.StandardError.ReadLine()) != null)
                {
                    _output.WriteLine(line);
                }
            }

            Assert.Equal(0, p.ExitCode);
        }

        private static string GetSolutionDirectoryPath()
        {
            var pathParts = typeof(UpdaterTests).Assembly.Location.Split(Path.DirectorySeparatorChar).AsSpan();
            var testDirectoryIndex = pathParts.LastIndexOf("test");
            var slnDirectoryPath = Path.Combine(pathParts.Slice(0, testDirectoryIndex).ToArray());

            return slnDirectoryPath;
        }
    }
}
