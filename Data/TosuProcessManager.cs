using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsuOscVRC.Data
{
    public class TosuProcessManager : IDisposable
    {
        private Process? _process;
        private readonly string _exePath;
        private readonly string _host;
        private readonly int _port;

        public bool IsRunning => _process != null && !_process.HasExited;

        public TosuProcessManager(string exePath, string host, int port)
        {
            _exePath = exePath;
            _host = host;
            _port = port;
        }

        public bool IsPortInUse()
        {
            try
            {
                using var tcp = new TcpClient();
                var result = tcp.BeginConnect(_host, _port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                if (success) { tcp.EndConnect(result); return true; }
                return false;
            }
            catch { return false; }
        }

        public string? Start(bool hidden = true)
        {
            if (IsPortInUse()) return null; // tosu already running

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.IsPathRooted(_exePath)
                ? _exePath
                : Path.GetFullPath(Path.Combine(basePath, _exePath));

            if (!File.Exists(fullPath))
            {
                return $"tosu.exe not found at:\n{fullPath}\n\n" +
                       "Download tosu from:\nhttps://github.com/tosuapp/tosu/releases\n" +
                       "and place it in the tosu/ subfolder.";
            }

            try
            {
                // Write config to disable browser opening
                EnsureNoBrowser(Path.GetDirectoryName(fullPath)!);

                var psi = new ProcessStartInfo
                {
                    FileName = fullPath,
                    WorkingDirectory = Path.GetDirectoryName(fullPath) ?? ".",
                    UseShellExecute = true,
                    CreateNoWindow = hidden,
                    WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                };

                _process = Process.Start(psi);
                return _process == null ? "Failed to start tosu process." : null;
            }
            catch (Exception ex)
            {
                return $"Failed to start tosu: {ex.Message}";
            }
        }

        /// Writes/updates tosu.env and tosu.toml to set openDashboardOnStartup = false
        /// </summary>
        private void EnsureNoBrowser(string tosuDir)
        {
            // Support for tosu v4 (.env format)
            var envPath = Path.Combine(tosuDir, "tosu.env");
            try
            {
                if (File.Exists(envPath))
                {
                    string content = File.ReadAllText(envPath);
                    if (content.Contains("OPEN_DASHBOARD_ON_STARTUP"))
                    {
                        content = Regex.Replace(content,
                            @"OPEN_DASHBOARD_ON_STARTUP\s*=\s*\w+",
                            "OPEN_DASHBOARD_ON_STARTUP=false");
                    }
                    else
                    {
                        content += "\nOPEN_DASHBOARD_ON_STARTUP=false\n";
                    }
                    File.WriteAllText(envPath, content);
                }
                else
                {
                    File.WriteAllText(envPath, "OPEN_DASHBOARD_ON_STARTUP=false\n");
                }
            }
            catch { }

            // Support for older tosu v3 (.toml format)
            var tomlPath = Path.Combine(tosuDir, "tosu.toml");
            try
            {
                if (File.Exists(tomlPath))
                {
                    string content = File.ReadAllText(tomlPath);
                    if (Regex.IsMatch(content, @"openDashboardOnStartup\s*=\s*\w+", RegexOptions.IgnoreCase))
                    {
                        content = Regex.Replace(content,
                            @"openDashboardOnStartup\s*=\s*\w+",
                            "openDashboardOnStartup = false",
                            RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        content += "\nopenDashboardOnStartup = false\n";
                    }
                    File.WriteAllText(tomlPath, content);
                }
                // Only create toml if neither exists to avoid creating dummy files for v4 users
                else if (!File.Exists(envPath))
                {
                    File.WriteAllText(tomlPath, "[dangerous]\nopenDashboardOnStartup = false\n");
                }
            }
            catch { /* ignore config write errors */ }
        }

        public async Task<bool> WaitForReady(int timeoutMs = 15000)
        {
            var sw = Stopwatch.StartNew();
            while (timeoutMs == Timeout.Infinite || sw.ElapsedMilliseconds < timeoutMs)
            {
                if (_process != null && _process.HasExited) return false;
                if (IsPortInUse()) return true;
                await Task.Delay(500);
            }
            return false;
        }

        public void Stop()
        {
            if (_process == null) return;
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    _process.WaitForExit(3000);
                }
            }
            catch { }
            finally { _process.Dispose(); _process = null; }
        }

        public void Dispose() => Stop();
    }
}
