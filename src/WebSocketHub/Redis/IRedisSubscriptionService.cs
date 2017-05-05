using System.Threading.Tasks;

namespace WebSocketHub.Redis
{
    public interface IRedisSubscriptionService
    {
        Task SubscribeTo(string topic, Connection client);
        Task UnsubscribeFrom(string topic, Connection client);
        Task UnsubscribeFromAll(Connection client);
    }
}