using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebSocketHub
{
    public abstract class WebSocketConnectionHandler
    {
        protected readonly ILogger<WebSocketConnectionHandler> _logger;

        protected WebSocketConnectionHandler(ILogger<WebSocketConnectionHandler> logger)
        {
            _logger = logger;
        }
        
        public virtual Task OnConnectionAccepted(Connection connection)
        {
            _logger.LogInformation("WebSocketConnectionHandler.OnConnectionAccepted()");
            return Task.CompletedTask;
        }

        public virtual Task OnConnectionClosed(Connection connection)
        {
            _logger.LogInformation("WebSocketConnectionHandler.OnConnectionClosed()");
            return Task.CompletedTask;
        }

        public async Task ReceiveAsync(Connection connection, Connection.MessageType messageType,
            ArraySegment<byte> message, CancellationToken cancellationToken)
        {
            if (messageType == Connection.MessageType.Binary)
            {
                await ProcessBinaryMessageReceivedAsync(connection, message, cancellationToken);
            }
            else if (messageType == Connection.MessageType.Text)
            {
                await ProcessTextMessageReceivedAsync(connection, message, cancellationToken);
            }
        }

        protected abstract Task ProcessTextMessageReceivedAsync(Connection connection, ArraySegment<byte> message, CancellationToken cancellationToken);
        protected abstract Task ProcessBinaryMessageReceivedAsync(Connection connection, ArraySegment<byte> messageBuffer, CancellationToken cancellationToken);
    }
}