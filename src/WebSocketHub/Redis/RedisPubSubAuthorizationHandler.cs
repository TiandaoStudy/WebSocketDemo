using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace WebSocketHub.Redis
{
    public class RedisPubSubAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, string>
    {
        private readonly IRedisPubSubAuthorizationService _authorizationService;

        public RedisPubSubAuthorizationHandler(IRedisPubSubAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement redisOperationRequirement, string redisTopic)
        {
            if (await _authorizationService.Allows(context.User, redisOperationRequirement.ToRedisOperation(),
                redisTopic))
            {
                context.Succeed(redisOperationRequirement);
            }
        }
    }
}