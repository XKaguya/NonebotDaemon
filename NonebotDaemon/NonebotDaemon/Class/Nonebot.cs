using System.Diagnostics;
using System.Text;
using log4net;
using NonebotDaemon.Core;
using NonebotDaemon.Enum;

namespace NonebotDaemon.Class
{
    public class Nonebot
    {
        public string Entry {get; set;} = String.Empty;
        public string Folder {get; set;} = string.Empty;
        public bool IsUsingVirtualEnvironment {get; set;} = false;
        public bool WebsocketFirstHeartbeat {get; set;} = false;
        private int Pid {get; set;} = 0;
        public Dictionary<Command, DateTime> Heartbeat {get; set;} = new ();
        private static readonly ILog Log = LogManager.GetLogger(typeof(Nonebot));

        public dynamic StartNonebot()
        {
            if (Folder == string.Empty)
            {
                throw new DirectoryNotFoundException("The directory could not be found.");
            }
                
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();

                var folder = @$"{Folder.Trim('"')}";
                
                Directory.SetCurrentDirectory(folder);
                
                if (IsUsingVirtualEnvironment)
                {
                    startInfo.FileName = "poetry";
                    startInfo.Arguments = $"run python {Entry}";
                }
                else
                {
                    startInfo.FileName = "python";
                    startInfo.Arguments = Entry;
                }
                
                startInfo.UseShellExecute = true;
                
                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    Pid = process.Id;
                    Console.WriteLine($"Starting Nonebot with PID: {Pid}");
                    Console.WriteLine($"Starting Nonebot with StartInfo: {startInfo.FileName} {startInfo.Arguments}");
                    
                    return new { Success = true, ProcessId = Pid };
                }
                else
                {
                    Console.WriteLine("Failed to start Nonebot.");
                    return new { Success = false, ErrorMessage = "Failed to start the Python process." };
                }
            }
            catch (Exception ex)
            {
                return new { Success = false, ErrorMessage = ex };
            }
        }
        
        private Dictionary<ResponseType, bool> GetFlags()
        {
            Dictionary<ResponseType, bool> flags = new Dictionary<ResponseType, bool>();

            if (WebsocketFirstHeartbeat)
            {
                try
                {
                    Process process = Process.GetProcessById(Pid);
                    if (process.Responding)
                    {
                        flags.Add(ResponseType.Responding, true);
                    }
                    else
                    {
                        flags.Add(ResponseType.Responding, false);
                    }
                }
                catch (ArgumentException ex)
                {
                    flags.Add(ResponseType.Responding, false);
                }

                if ((DateTime.Now - Heartbeat[Command.ClientHeartbeat]).TotalSeconds <= GlobalVariables.HeartbeatInterval)
                {
                    flags.Add(ResponseType.ReceiveHeartbeat, true);
                }
            }
            
            return flags;
        }
        
        public bool IsRunning()
        {
            if (WebsocketFirstHeartbeat)
            {
                return GetFlags()
                    .Count(flag => (flag.Key == ResponseType.Responding || flag.Key == ResponseType.ReceiveHeartbeat) && flag.Value) == 2;
            }
            
            // This line should not be called in normal case.
            return false;
        }
    }
}