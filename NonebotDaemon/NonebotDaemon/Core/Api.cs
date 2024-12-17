using log4net;
using NonebotDaemon.Class;

namespace NonebotDaemon.Core
{
    public static class Api
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Api));

        public static Nonebot? Nonebot = null;
        
        public static bool StartNonebotDaemon()
        {
            Nonebot nonebot = new Nonebot();
            nonebot.Entry = GlobalVariables.NonebotEntry;
            nonebot.Folder = GlobalVariables.NonebotFolder;
            nonebot.IsUsingVirtualEnvironment = GlobalVariables.IsUsingVirtualEnvironment;

            dynamic result = nonebot.StartNonebot();
            if (result.Success)
            {
                Log.Info($"Nonebot Daemon successfully started nonebot.");
                Nonebot = nonebot;
                
                return true;
            }
            
            if (!result.Success)
            {
                if (result.ErrorMessage != null)
                {
                    Log.Error(result.ErrorMessage);
                }
            }

            return false;
        }
    }
}