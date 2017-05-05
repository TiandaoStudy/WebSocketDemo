using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WebSocketHub.Redis;

namespace WebSocketHub
{
    public static class WebSocketHubMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketHub(this IApplicationBuilder app, IdentityServerAuthenticationOptions identityServerAuthenticationOptions)
        {
            app.UseWebSocketAccessTokenExtractor();
            app.UseIdentityServerAuthentication(identityServerAuthenticationOptions);
            app.UseWebSockets();
            app.UseMiddleware<WebSocketHubMiddleware>();
            return app;
        }
        
        public static IApplicationBuilder UseWebSocketAccessTokenExtractor(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebSocketAccessTokenExtractorMiddleware>();
            return app;
        }

        // todo: set up some configuration options for redis connection.
        public static IServiceCollection AddWebSocketHub(this IServiceCollection services, string redisConnectionString = "localhost:6379")
        {
            services.AddAuthentication();
            services.AddAuthorization();
            services.AddScoped<IAuthorizationHandler, RedisPubSubAuthorizationHandler>();
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