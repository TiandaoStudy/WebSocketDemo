using System.Security.Claims;
using System.Threading.Tasks;

namespace WebSocketHub.Redis
{
    public interface IRedisPubSubAuthorizationService
    {
        Task<bool> Allows(ClaimsPrincipal user, RedisPubSubOperation operation, string topicName);
    }
}