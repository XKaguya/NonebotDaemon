using System.ComponentModel;

namespace NonebotDaemon.Core
{
    public class GlobalVariables
    {
        [Description("Set to true to allow NonebotDaemon auto update. \nDefault value: true")]
        public static bool AutoUpdate {get; set;}
        [Description("Nonebot heartbeat interval time. (Seconds) \nDefault value: 15")]
        public static ushort HeartbeatInterval { get; set; } = 15;
        [Description("Set to true if using Poetry or etc. \nDefault value: false")]
        public static bool IsUsingVirtualEnvironment { get; set; } = false;
        [Description("Nonebot entry. \nDefault value: bot.py")]
        public static string NonebotEntry { get; set; } = "bot.py";
        [Description("Nonebot folder. \nDefault value: PATH_TO_FILE")]
        public static string NonebotFolder { get; set; } = "PATH_TO_FILE";
        [Description("Nonebot log file folder. \nDefault value: PATH_TO_FILE")]
        public static string WebSocketListenerAddress { get; set; } = "http://localhost";
        [Description("WebSocket Listener port. \nDefault value: 8890")]
        public static ushort WebSocketListenerPort { get; set; } = 8890;
    }
}