using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WebSocketHub
{
    public class WebSocketMiddlewareResources
    {
        public static readonly string WebSocket_Request_Initiated = "WebSocket request initiated.";
        public static readonly string WebSocket_Closing = "WebSocket is closing.";
        public static readonly string WebSocket_On_Exception = "Exception occured, removing client from the connections. Error message: {0}";
    }

    public class WebSocketHubMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketHubMiddleware> _logger;
        private readonly WebSocketConnectionHandler _connectionHandler;

        public WebSocketHubMiddleware(RequestDelegate next, ILogger<WebSocketHubMiddleware> logger,
            WebSocketConnectionHandler connectionHandler)
        {
            _next = next;
            _logger = logger;
            _connectionHandler = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            _logger.LogInformation(WebSocketMiddlewareResources.WebSocket_Request_Initiated);
            using (var connection = new Connection(await context.WebSockets.AcceptWebSocketAsync(), context.User))
            {
                var cancellationToken = context.RequestAborted;
                try
                {
                    await _connectionHandler.OnConnectionAccepted(connection);
                    await connection.ReceiveMessages(
                        async (messageType, message) => { await _connectionHandler.ReceiveAsync(connection, messageType, message, cancellationToken); },
                        async () => 
                        {
                            await _connectionHandler.OnConnectionClosed(connection);
                            _logger.LogInformation(WebSocketMiddlewareResources.WebSocket_Closing);
                        },
                        cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(WebSocketMiddlewareResources.WebSocket_On_Exception, exception.Message);
                    await _connectionHandler.OnConnectionClosed(connection);
                }
            }
        }
    }
}