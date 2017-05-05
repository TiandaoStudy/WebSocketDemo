using System;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace WebSocketHub.Redis
{
    public static class RedisPubSubOperationAuthorizationRequirements
    {
        public static OperationAuthorizationRequirement Subscribe =
            new OperationAuthorizationRequirement {Name = "Subscribe"};

        public static OperationAuthorizationRequirement Unsubscribe =
            new OperationAuthorizationRequirement {Name = "Unsubscribe"};

        public static OperationAuthorizationRequirement Publish =
            new OperationAuthorizationRequirement {Name = "Publish"};

        public static RedisPubSubOperation ToRedisOperation(
            this OperationAuthorizationRequirement redisPubSubOperationRequirement)
        {
            switch (redisPubSubOperationRequirement.Name)
            {
                case "Subscribe": return RedisPubSubOperation.Subscribe;
                case "Unsubscribe": return RedisPubSubOperation.Unsubscribe;
                case "Publish": return RedisPubSubOperation.Publish;
            }

            throw new ArgumentException("Invalid redis requirement name.");
        }
    }
}