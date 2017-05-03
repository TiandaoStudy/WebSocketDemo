using System;
using System.IO;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketHub
{
    public class Connection : IDisposable
    {
        private WebSocket Transport { get; }

        public Guid Id { get; }
        public ClaimsPrincipal User { get; }
        
        public bool IsOpen => Transport.State == WebSocketState.Open;

        public Connection(WebSocket socket, ClaimsPrincipal user)
        {
            Id = Guid.NewGuid();
            Transport = socket ?? throw new ArgumentNullException(nameof(socket));
            User = user;
        }

        public async Task CloseAsync(string errorMessage = null)
        {
            if (Transport.State == WebSocketState.Open || Transport.State == WebSocketState.CloseReceived)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    await Transport.CloseAsync(WebSocketCloseStatus.InternalServerError, errorMessage,
                        CancellationToken.None);
                }
                else
                {
                    await Transport.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.",
                        CancellationToken.None);
                }
            }
        }

        public async Task SendAsync(byte[] message)
        {
            if (IsOpen)
            {
                await Transport.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                    CancellationToken.None);
            }
        }
        
        public async Task ReceiveMessages(Action<MessageType, ArraySegment<byte>> onMessageReceived, Action onConnectionClosed, CancellationToken cancellationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);

            while (IsOpen)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // todo: check memory usage with many connections and high message count
                    // todo: is it possible to use ArrayPool?
                    // todo: memorystream initial size considerations?
                    WebSocketReceiveResult messageResult = null;
                    do
                    {
                        messageResult = await Transport.ReceiveAsync(buffer, cancellationToken);
                        await memoryStream.WriteAsync(buffer.Array, 0, messageResult.Count, cancellationToken);
                    } while (!messageResult.EndOfMessage);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    ArraySegment<byte> messageBytes;
                    bool getBufferIsSuccessful = memoryStream.TryGetBuffer(out messageBytes);
                    if (!getBufferIsSuccessful)
                    {
                        messageBytes = new ArraySegment<byte>(memoryStream.ToArray());
                    }

                    if (cancellationToken.IsCancellationRequested || messageResult.MessageType == WebSocketMessageType.Close)
                    {
                        onConnectionClosed();
                    }
                    else
                    {
                        var messageType = messageResult.MessageType == WebSocketMessageType.Binary
                            ? MessageType.Binary
                            : MessageType.Text;

                        onMessageReceived(messageType, messageBytes);
                    }
                }
            }
        }

        public void Dispose()
        {
            Transport.Dispose();
        }

        public enum MessageType
        {
            Binary,
            Text
        }
    }
}