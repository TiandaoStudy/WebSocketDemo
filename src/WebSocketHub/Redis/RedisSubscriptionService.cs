using ConcurrentCollections;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace WebSocketHub.Redis
{
    public class RedisSubscriptionService : IRedisSubscriptionService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<RedisSubscriptionService> _logger;
        private readonly IConnectionMultiplexer _redisConnection;

        private readonly ConcurrentDictionary<string, ConcurrentHashSet<Connection>> _interestingTopics =
            new ConcurrentDictionary<string, ConcurrentHashSet<Connection>>();

        private readonly HashSet<string> _redisSubscriptions = new HashSet<string>();
        private readonly object _lockObj = new object();

        public RedisSubscriptionService(IConnectionMultiplexer redisConnection,
            IAuthorizationService authorizationService, ILogger<RedisSubscriptionService> logger)
        {
            _redisConnection = redisConnection;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task SubscribeTo(string topic, Connection client)
        {
            if (await _authorizationService.AuthorizeAsync(client.User, topic, RedisPubSubOperationAuthorizationRequirements.Subscribe))
            {
                // if there are no subscribers for the topic, we create a new collection and we add the client to it
                var subscribers = _interestingTopics.GetOrAdd(topic, new ConcurrentHashSet<Connection>());
                subscribers.Add(client);

                CreateRedisSubscription(topic);
                _logger.LogInformation("Client {0} subscribed to topic {1}", client.Id, topic);
            }
            else
            {
                _logger.LogError("Unauthorized subscription attempt for topic {0} by client {1}", topic, client.Id);
            }
        }

        public async Task UnsubscribeFrom(string topic, Connection client)
        {
            if (await _authorizationService.AuthorizeAsync(client.User, topic, RedisPubSubOperationAuthorizationRequirements.Unsubscribe))
            {
                ConcurrentHashSet<Connection> interestedClients;
                if (_interestingTopics.TryGetValue(topic, out interestedClients))
                {
                    interestedClients.TryRemove(client);
                }
                _logger.LogInformation("Client {0} unsubscribed from topic {1}", client.Id, topic);
            }
            else
            {
                _logger.LogError("Unauthorized unsubscription attempt from topic {0} by client {1}", topic, client.Id);
            }
        }

        public async Task UnsubscribeFromAll(Connection client)
        {
            // todo: what happens when there are a LOT of topics? should we maintain a different collection for the topics specific for the client?
            var topics = _interestingTopics.Keys;
            foreach (var topic in topics)
            {
                await UnsubscribeFrom(topic, client);
            }
        }

        private void CreateRedisSubscription(string topic)
        {
            if (!_redisSubscriptions.Contains(topic))
            {
                lock (_lockObj)
                {
                    if (!_redisSubscriptions.Contains(topic))
                    {
                        var previousBroadcastToInterestedClients = Task.CompletedTask;

                        _redisConnection.GetSubscriber()
                            .Subscribe(topic, async (channel, data) =>
                            {
                                await previousBroadcastToInterestedClients;
                                ConcurrentHashSet<Connection> clientsToNotify;
                                if (_interestingTopics.TryGetValue(channel, out clientsToNotify))
                                {
                                    var tasks = new List<Task>(clientsToNotify.Count);
                                    foreach (var client in clientsToNotify)
                                    {
                                        // there is an implicit conversion from RedisValue to byte[] which does not need copying or string encoding.
                                        // we can just send this to the websocket
                                        tasks.Add(client.SendAsync(data));
                                    }
                                    previousBroadcastToInterestedClients = Task.WhenAll(tasks);
                                }
                            });

                        _redisSubscriptions.Add(topic);
                    }
                }
            }
        }
    }
}