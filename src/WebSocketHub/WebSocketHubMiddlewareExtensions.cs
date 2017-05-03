using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace WebSocketHub
{
    public static class WebSocketHubMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketHub(this IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.UseMiddleware<WebSocketHubMiddleware>();
            return app;
        }

        // todo: set up some configuration options for redis connection.
        public static IServiceCollection AddWebSocketHub(this IServiceCollection services, string redisConnectionString = "localhost:6379")
        {
            // setup redis
            services.AddSingleton<IConnectionMultiplexer>(
                provider => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddTransient<ISubscriber>(
                provider => provider.GetService<IConnectionMultiplexer>().GetSubscriber());

            // websocket handling
            services.AddSingleton<WebSocketConnectionHandler, HubConnectionHandler>();
            services.AddTransient<IRedisSubscriptionService, RedisSubscriptionService>();
            return services;
        }
    }
}