using System.Net;
using System.Net.WebSockets;
using System.Text;
using log4net;
using NonebotDaemon.Enum;

namespace NonebotDaemon.Core
{
    public class WebSocketServer
    {
        private static HttpListener? _listener;
        private static CancellationTokenSource? _cancellationTokenSource;
        private static readonly ILog Log = LogManager.GetLogger(typeof(WebSocketServer));

        public static async Task StartWebSocketServerAsync(string[] prefixes)
        {
            _listener = new HttpListener();
            _cancellationTokenSource = new CancellationTokenSource();

            if (prefixes.Length != 0)
            {
                foreach (string prefix in prefixes)
                {
                    _listener.Prefixes.Add(prefix);
                }
            }
            else
            {
                throw new WebSocketException("Prefixes not defined.");
            }

            _listener.Start();
            Log.Info("WebSocket server started.");

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        ProcessWebSocketRequest(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _listener.Close();
                Log.Info("WebSocket server stopped.");
            }
        }

        public static void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private static async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            WebSocketContext? webSocketContext = null;

            try
            {
                webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
                Log.Debug($"Client connected.");

                WebSocket webSocket = webSocketContext.WebSocket;

                await HandleWebSocketMessages(webSocket);
            }
            catch (WebSocketException ex)
            {
                Log.Error($"WebSocket error: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Log.Error($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static async Task HandleWebSocketMessages(WebSocket webSocket)
        {
            try
            {
                byte[] buffer = new byte[50000];

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        ProcessClientMessage(webSocket, message);

                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log.Debug("WebSocket close request received, but not closing.");
                        // webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); 
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Log.Error($"WebSocket error: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Log.Error($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        private static void ProcessClientMessage(WebSocket webSocket, string receivedMessage)
        {
            string[] messageParts = receivedMessage.Split(' ');

            if (System.Enum.TryParse(messageParts[0], out Command command))
            {
                switch (command)
                {
                    case Command.ClientHeartbeat:
                        if (Api.Nonebot != null)
                        {
                            if (!Api.Nonebot.WebsocketFirstHeartbeat)
                            {
                                Console.WriteLine("Received first heartbeat.");
                                Api.Nonebot.WebsocketFirstHeartbeat = true;
                            }
                            
                            Api.Nonebot.Heartbeat.Clear();
                            Api.Nonebot.Heartbeat.Add(Command.ClientHeartbeat, DateTime.Now);
                        }
                        else
                        {
                            Log.Error($"Nonebot object is null. Please run Nonebot.StartNonebotDaemon() first.");
                        }
                        
                        break;
                    
                    default:
                        Log.Error($"Unknown Command: {command}");
                        break;
                }
            }
            else
            {
                Log.Error("Invalid command format.");
            }
        }
    }
}