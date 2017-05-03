using ConcurrentCollections;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebSocketHub
{
    public class RedisSubscriptionService : IRedisSubscriptionService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ConcurrentDictionary<string, ConcurrentHashSet<Connection>> _interestingTopics = new ConcurrentDictionary<string, ConcurrentHashSet<Connection>>();
        private readonly HashSet<string> _redisSubscriptions = new HashSet<string>();
        private readonly object _lockObj = new object();

        public RedisSubscriptionService(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
        }

        public void SubscribeTo(string topic, Connection client)
        {
            // if there are no subscribers for the topic, we create a new collection and we add the client to it
            var subscribers = _interestingTopics.GetOrAdd(topic, new ConcurrentHashSet<Connection>());
            subscribers.Add(client);

            SubscribeToRedisTopic(topic);
        }

        public void UnsubscribeFrom(string topic, Connection client)
        {
            ConcurrentHashSet<Connection> interestedClients;
            if (_interestingTopics.TryGetValue(topic, out interestedClients))
            {
                interestedClients.TryRemove(client);
            }
        }

        public void UnsubscribeFromAll(Connection client)
        {
            // todo: what happens when there are a LOT of topics? should we maintain a different collection for the topics specific for the client?
            var topics = _interestingTopics.Keys;
            foreach (var topic in topics)
            {
                UnsubscribeFrom(topic, client);
            }
        }

        private void SubscribeToRedisTopic(string topic)
        {
            if (!_redisSubscriptions.Contains(topic))
            {
                lock (_lockObj)
                {
                    if (!_redisSubscriptions.Contains(topic))
                    {
                        var previousBroadcastToInterestedClients = Task.CompletedTask;

                        _redisConnection.GetSubscriber().Subscribe(topic, async (channel, data) =>
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