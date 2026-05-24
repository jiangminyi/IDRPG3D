using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public enum IDRPG3DLocalProcessState
    {
        Stopped,
        Starting,
        Running,
        Ready,
        Exited
    }

    public readonly struct IDRPG3DLocalServerCommand
    {
        public IDRPG3DLocalServerCommand(string fileName, string arguments, string workingDirectory)
        {
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
        }

        public string FileName { get; }
        public string Arguments { get; }
        public string WorkingDirectory { get; }
    }

    public sealed class IDRPG3DLocalServerLogBuffer
    {
        private readonly object gate = new object();
        private readonly int maxLines;
        private readonly Queue<string> lines = new Queue<string>();

        public IDRPG3DLocalServerLogBuffer(int maxLines = 500)
        {
            this.maxLines = Mathf.Max(1, maxLines);
        }

        public string[] Snapshot
        {
            get
            {
                lock (gate)
                {
                    return lines.ToArray();
                }
            }
        }

        public void Append(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            lock (gate)
            {
                lines.Enqueue(line);
                while (lines.Count > maxLines)
                {
                    lines.Dequeue();
                }
            }
        }

        public void Clear()
        {
            lock (gate)
            {
                lines.Clear();
            }
        }
    }

    public sealed class IDRPG3DLocalServerProcess : IDisposable
    {
        private readonly IDRPG3DLocalServerLogBuffer logBuffer;
        private readonly int[] ownedPorts;
        private readonly string[] ownedProcessNames;
        private Process process;
        private bool stopRequested;

        public IDRPG3DLocalServerProcess(
            string displayName,
            IDRPG3DLocalServerLogBuffer logBuffer,
            int[] ownedPorts = null,
            string[] ownedProcessNames = null)
        {
            DisplayName = displayName;
            this.logBuffer = logBuffer;
            this.ownedPorts = ownedPorts ?? Array.Empty<int>();
            this.ownedProcessNames = ownedProcessNames ?? Array.Empty<string>();
        }

        public string DisplayName { get; }
        public IDRPG3DLocalProcessState State { get; private set; } = IDRPG3DLocalProcessState.Stopped;
        public int? ProcessId => process != null && !process.HasExited ? process.Id : null;
        public bool IsRunning => process != null && !process.HasExited;

        public static IDRPG3DLocalServerCommand CreateGameServerCommand(string repositoryRoot)
        {
            return new IDRPG3DLocalServerCommand(
                "dotnet",
                "run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- -m Develop -g 1",
                NormalizePath(repositoryRoot));
        }

        public static IDRPG3DLocalServerCommand CreateMongoExpressCommand(string repositoryRoot)
        {
#if UNITY_EDITOR_WIN
            const string shell = "powershell.exe";
#else
            const string shell = "pwsh";
#endif
            const string arguments = "-NoProfile -ExecutionPolicy Bypass -File \"Scripts/start-mongo-express.ps1\"";
            return new IDRPG3DLocalServerCommand(shell, arguments, NormalizePath(repositoryRoot));
        }

        public void Start(IDRPG3DLocalServerCommand command)
        {
            if (IsRunning)
            {
                AppendLog("Already running.");
                return;
            }

            KillOwnedPortProcesses();

            State = IDRPG3DLocalProcessState.Starting;
            stopRequested = false;
            AppendLog($"> {command.FileName} {command.Arguments}");

            var startInfo = new ProcessStartInfo(command.FileName, command.Arguments)
            {
                WorkingDirectory = command.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, args) => AppendLog(args.Data);
            process.ErrorDataReceived += (_, args) => AppendLog(args.Data);
            process.Exited += (_, _) =>
            {
                State = stopRequested ? IDRPG3DLocalProcessState.Stopped : IDRPG3DLocalProcessState.Exited;
                AppendLog($"Process exited. ExitCode={SafeExitCode()}");
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                State = IDRPG3DLocalProcessState.Running;
                AppendLog($"Started. PID={process.Id}");
            }
            catch (Exception exception)
            {
                State = IDRPG3DLocalProcessState.Exited;
                AppendLog($"Start failed: {exception.Message}");
                process.Dispose();
                process = null;
            }
        }

        public void Stop()
        {
            if (process == null)
            {
                State = IDRPG3DLocalProcessState.Stopped;
                AppendLog("No process to stop.");
                KillOwnedPortProcesses();
                return;
            }

            if (process.HasExited)
            {
                State = IDRPG3DLocalProcessState.Exited;
                AppendLog($"Already exited. ExitCode={SafeExitCode()}");
                DisposeProcess();
                KillOwnedPortProcesses();
                return;
            }

            try
            {
                AppendLog("Stopping...");
                stopRequested = true;
                KillProcessTree(process.Id);
                process.WaitForExit(3000);
                KillOwnedPortProcesses();
                State = IDRPG3DLocalProcessState.Stopped;
                AppendLog("Stopped.");
            }
            catch (Exception exception)
            {
                AppendLog($"Stop failed: {exception.Message}");
            }
            finally
            {
                DisposeProcess();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        private static void KillProcessTree(int processId)
        {
#if UNITY_EDITOR_WIN
            using var taskKill = Process.Start(new ProcessStartInfo("taskkill", $"/PID {processId} /T /F")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            taskKill?.WaitForExit(3000);
#else
            Process.GetProcessById(processId).Kill();
#endif
        }

        private int SafeExitCode()
        {
            try
            {
                return process?.ExitCode ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private void AppendLog(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                UpdateReadyState(line);
                logBuffer.Append($"[{DateTime.Now:HH:mm:ss}] [{DisplayName}] {line}");
            }
        }

        private void UpdateReadyState(string line)
        {
            if (State != IDRPG3DLocalProcessState.Running)
            {
                return;
            }

            var lower = line.ToLowerInvariant();
            if (lower.Contains("server listening") ||
                lower.Contains("database connected") ||
                lower.Contains("database idrpg3d_dev connected"))
            {
                State = IDRPG3DLocalProcessState.Ready;
            }
        }

        private void DisposeProcess()
        {
            if (process == null)
            {
                return;
            }

            process.Dispose();
            process = null;
        }

        private void KillOwnedPortProcesses()
        {
            if (ownedPorts.Length == 0 || ownedProcessNames.Length == 0)
            {
                return;
            }

            foreach (var processId in IDRPG3DLocalPortOwners.GetProcessIdsByPorts(ownedPorts))
            {
                if (ProcessId.HasValue && processId == ProcessId.Value)
                {
                    continue;
                }

                try
                {
                    using var portProcess = Process.GetProcessById(processId);
                    if (!IDRPG3DLocalPortOwners.IsExpectedProcessName(portProcess.ProcessName, ownedProcessNames))
                    {
                        AppendLog($"Port owner PID={processId} is {portProcess.ProcessName}, skip.");
                        continue;
                    }

                    AppendLog($"Cleaning leftover process {portProcess.ProcessName}. PID={processId}");
                    KillProcessTree(processId);
                }
                catch (Exception exception)
                {
                    AppendLog($"Clean leftover PID={processId} failed: {exception.Message}");
                }
            }
        }
    }

    public static class IDRPG3DLocalProjectPaths
    {
        public static string RepositoryRoot
        {
            get
            {
                var unityProjectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                var clientRoot = Directory.GetParent(unityProjectRoot ?? string.Empty)?.FullName;
                return Directory.GetParent(clientRoot ?? string.Empty)?.FullName?.Replace('\\', '/') ?? string.Empty;
            }
        }
    }

    public static class IDRPG3DLocalPortOwners
    {
        private static readonly Regex Whitespace = new Regex(@"\s+", RegexOptions.Compiled);

        public static int[] GetProcessIdsByPorts(IEnumerable<int> ports)
        {
            using var netstat = Process.Start(new ProcessStartInfo("netstat", "-ano")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (netstat == null)
            {
                return Array.Empty<int>();
            }

            var output = netstat.StandardOutput.ReadToEnd();
            netstat.WaitForExit(3000);
            return ParseProcessIdsByPorts(output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries), ports);
        }

        public static int[] ParseProcessIdsByPorts(IEnumerable<string> netstatLines, IEnumerable<int> ports)
        {
            var portSet = new HashSet<int>(ports);
            var processIds = new HashSet<int>();

            foreach (var line in netstatLines)
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("TCP ", StringComparison.OrdinalIgnoreCase) &&
                    !trimmed.StartsWith("UDP ", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parts = Whitespace.Split(trimmed);
                if (parts.Length < 4)
                {
                    continue;
                }

                var localAddress = parts[1];
                var processIdText = parts[^1];
                if (!TryParsePort(localAddress, out var port) || !portSet.Contains(port))
                {
                    continue;
                }

                if (int.TryParse(processIdText, out var processId))
                {
                    processIds.Add(processId);
                }
            }

            return processIds.ToArray();
        }

        public static bool IsExpectedProcessName(string processName, IEnumerable<string> expectedNames)
        {
            return expectedNames.Any(expected => string.Equals(processName, expected, StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryParsePort(string localAddress, out int port)
        {
            port = 0;
            var colonIndex = localAddress.LastIndexOf(':');
            return colonIndex >= 0 &&
                   colonIndex < localAddress.Length - 1 &&
                   int.TryParse(localAddress.Substring(colonIndex + 1), out port);
        }
    }
}
