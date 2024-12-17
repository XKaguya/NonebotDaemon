using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Config;
using NonebotDaemon.Feature;

namespace NonebotDaemon.Core
{
    public class Init
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Init));
        
        public static async Task Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "NonebotDaemon.log4net.config";
        
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                XmlConfigurator.Configure(stream);
            }
            else
            {
                Log.Error($"Failed to find embedded resource: {resourceName}");
            }
            
            ProcessConfig.ParseConfig();
            
            AutoUpdate.StartAutoUpdateTask();
            
            string prefix = $"{GlobalVariables.WebSocketListenerAddress}:{GlobalVariables.WebSocketListenerPort}/";
            string[] prefixes = { prefix };
            
            Log.Info($"Trying to start WebSocket on: {prefix}");

            Log.Info("Starting NonebotDaemon...");
            Api.StartNonebotDaemon();
            
            var webSocketServerTask = Task.Run(() => WebSocketServer.StartWebSocketServerAsync(prefixes));
            
            await MonitorLoopAsync();
            
            await webSocketServerTask;
        }
        
        private static async Task MonitorLoopAsync()
        {
            while (true)
            {
                await CheckLoop();
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
        
        private static async Task CheckLoop()
        {
            if (Api.Nonebot != null && Api.Nonebot.WebsocketFirstHeartbeat)
            {
                if (!Api.Nonebot.IsRunning())
                {
                    Console.WriteLine("Nonebot is not running. Restarting...");
                    Log.Warn("Nonebot is not running. Restarting...");
                    
                    Process.GetProcessesByName("python").ToList().ForEach(process => process.Kill());
                    
                    Api.StartNonebotDaemon();
                }
            }
        }
    }
}