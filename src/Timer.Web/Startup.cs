using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebSocketHub;
using WebSocketHub.Redis;

namespace Timer.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketHub();
            services.AddTransient<IRedisPubSubAuthorizationService, DefaultPubSubAuthorizationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseWebSocketHub(new IdentityServerAuthenticationOptions
            {
                Authority = "http://localhost:5001",
                RequireHttpsMetadata = false,
                ApiName = "websocket-api"
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }

    public class DefaultPubSubAuthorizationService: IRedisPubSubAuthorizationService
    {
        public Task<bool> Allows(ClaimsPrincipal user, RedisPubSubOperation operation, string topicName)
        {
            // we just allow everything to everyone for demo purposes.
            return Task.FromResult(true);
        }
    }
}
