using NonebotDaemon.Core;

namespace NonebotDaemon
{
    public class Program
    {
        public const string ConfigFilePath = "config.xml";
        
        public const string Version = "1.0.0";
        
        public static async Task Main(string[] args)
        {
            await Init.Initialize();
        }
    }
}
