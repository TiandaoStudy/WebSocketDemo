namespace WebSocketHub
{
    public interface IRedisSubscriptionService
    {
        void SubscribeTo(string topic, Connection client);
        void UnsubscribeFrom(string topic, Connection client);
        void UnsubscribeFromAll(Connection client);
    }
}