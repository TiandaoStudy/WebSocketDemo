using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketHub.HubMessages;
using WebSocketHub.Redis;

namespace WebSocketHub
{
    public class HubConnectionHandler : WebSocketConnectionHandler
    {
        private readonly IRedisSubscriptionService _redisSubscriptionService;
        private readonly ConnectionList _connectedClients = new ConnectionList();

        public HubConnectionHandler(ILogger<HubConnectionHandler> logger, IRedisSubscriptionService redisSubscriptionService) : base(logger)
        {
            _redisSubscriptionService = redisSubscriptionService;
        }

        public override Task OnConnectionAccepted(Connection connection)
        {
            _connectedClients.AddClient(connection);
            return Task.CompletedTask;
        }

        public override async Task OnConnectionClosed(Connection connection)
        {
            try
            {
                await _redisSubscriptionService.UnsubscribeFromAll(connection);
                _connectedClients.RemoveClient(connection);
                await connection.CloseAsync();
            }
            catch (ConnectionList.ClientNotFoundException ex)
            {
                await connection.CloseAsync(ex.Message);
            }
        }
        
         protected override async Task ProcessTextMessageReceivedAsync(Connection connection, ArraySegment<byte> message,
            CancellationToken cancellationToken)
        {
            // TODO: omg hacky shit
            dynamic parsedMessage;
            using (var memoryStream = new MemoryStream(message.Array))
            using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
            {
                parsedMessage = JObject.Parse(await streamReader.ReadToEndAsync());
            }

            MessageType messageType = parsedMessage.messageType;
            string topic = parsedMessage.data.topic;
            switch (messageType)
            {
                case MessageType.Subscribe:
                    await _redisSubscriptionService.SubscribeTo(topic, connection);
                    break;
                case MessageType.Unsubscribe:
                    await _redisSubscriptionService.UnsubscribeFrom(topic, connection);
                    break;
                case MessageType.Message:
                    // not sure yet
                    throw new NotImplementedException();
                    break;
            }
        }

        protected override Task ProcessBinaryMessageReceivedAsync(Connection connection, ArraySegment<byte> messageBuffer,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}