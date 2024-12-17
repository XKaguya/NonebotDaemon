using System.Diagnostics;
using log4net;
using NonebotDaemon.Core;

namespace NonebotDaemon.Feature
{
    public class AutoUpdate
    {
        private static readonly string Author = "Xkaguya";
        private static readonly string Project = "NonebotDaemon";
        private static readonly string ExeName = "NonebotDaemon.exe";
        private static readonly string CurrentExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExeName);
        private static readonly string NewExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NonebotDaemon-New.exe");
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        
        private static readonly ILog Log = LogManager.GetLogger(typeof(AutoUpdate));

        public static void StartAutoUpdateTask()
        {
            Task.Run(async () => await AutoUpdateTask(CancellationTokenSource.Token));
        }

        private static async Task AutoUpdateTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                CheckAndUpdate();
                await Task.Delay(TimeSpan.FromHours(1), token);
            }
        }

        public static void CheckAndUpdate()
        {
            if (!GlobalVariables.AutoUpdate)
            {
                return;
            }
            
            try
            {
                string commonUpdaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommonUpdater.exe");

                if (!File.Exists(commonUpdaterPath))
                {
                    Log.Error("There's no CommonUpdater in the folder. Failed to update.");
                    return;
                }
                
                string arguments = $"{Project} {ExeName} {Author} {Program.Version} \"{CurrentExePath}\" \"{NewExePath}\"";
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = commonUpdaterPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true
                };

                Log.Debug($"Starting CommonUpdater with arguments: {arguments}");
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Log.Error("Failed to start CommonUpdater: Process.Start returned null.");
                    return;
                }
                
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (!string.IsNullOrEmpty(error))
                {
                    Log.Error($"CommonUpdater error: {error}");
                }
                    
                if (process.ExitCode != 0)
                {
                    Log.Error($"CommonUpdater exited with code {process.ExitCode}");
                }
                else
                {
                    Log.Debug("CommonUpdater started successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to start CommonUpdater: {ex.Message}");
            }
        }
    }
}
