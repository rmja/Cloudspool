using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PrintSpooler
{
    public static class ServiceControl
    {
        public static void Create(string serviceName, Dictionary<string, string> options) => Run("create", serviceName, options);
        public static void Delete(string serviceName) => Run("delete", serviceName);
        public static void Start(string serviceName) => Run("start", serviceName);
        public static void Stop(string serviceName) => Run("stop", serviceName);
        public static void Failure(string serviceName, Dictionary<string, string> options) => Run("failure", serviceName, options);

        public static ServiceState GetState(string serviceName)
        {
            using var sc = new Process();
            sc.StartInfo.FileName = "sc";
            sc.StartInfo.UseShellExecute = false;
            sc.StartInfo.RedirectStandardOutput = true;

            sc.StartInfo.ArgumentList.Add("query");
            sc.StartInfo.ArgumentList.Add(serviceName);

            sc.Start();
            sc.WaitForExit();
            if (sc.ExitCode == 0)
            {
                var output = sc.StandardOutput.ReadToEnd();

                var match = new Regex("STATE +: ([1-7])", RegexOptions.Multiline).Match(output);

                return (ServiceState)int.Parse(match.Groups[1].Value);
            }
            else if (sc.ExitCode == 1060)
            {
                return ServiceState.NotInstalled;
            }

            throw new NotSupportedException($"Unsupported sc exit code {sc.ExitCode}");
        }

        private static void Run(string command, string serviceName, Dictionary<string, string> options = null)
        {
            using var sc = new Process();
            sc.StartInfo.FileName = "sc";
            sc.StartInfo.UseShellExecute = true;
            sc.StartInfo.Verb = "runas";

            sc.StartInfo.ArgumentList.Add(command);
            sc.StartInfo.ArgumentList.Add(serviceName);

            if (options is object)
            {
                foreach (var option in options)
                {
                    sc.StartInfo.ArgumentList.Add($"{option.Key}=");
                    sc.StartInfo.ArgumentList.Add(option.Value);
                }
            }

            sc.Start();
            sc.WaitForExit();
            if (sc.ExitCode != 0)
            {
                throw new Exception($"Failed to run {command}, got exit code {sc.ExitCode}");
            }
        }
    }

    // https://docs.microsoft.com/en-us/windows/win32/services/service-status-transitions
    public enum ServiceState
    {
        NotInstalled = 0,
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7
    }
}
