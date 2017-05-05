using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddInMemoryApiResources(IdentityConfiguration.GetResources())
                .AddInMemoryIdentityResources(IdentityConfiguration.GetIdentityResources())
                .AddInMemoryClients(IdentityConfiguration.GetClients())
                .AddTestUsers(IdentityConfiguration.GetTestUsers());
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

        }
    }
    
    public static class IdentityConfiguration
    {
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "javascript-client",
                    ClientName = "Javascript Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { "http://localhost:5000/callback.html" },
                    PostLogoutRedirectUris = { "http://localhost:5000/index.html" },
                    AllowedCorsOrigins = { "http://localhost:5000" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "websocket_api.subscribe"
                    }
                }
            };
        }

        public static IEnumerable<ApiResource> GetResources()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "websocket-api",
                    ApiSecrets = { new Secret("websocket_api_secret".Sha256()) },
                    UserClaims = { "project:notifications:subscribe", "project:notifications:unsubscribe", "project:notifications:publish" },
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "websocket_api.subscribe",
                            DisplayName = "Subscribe to channels",
                            Required = true
                        }
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static List<TestUser> GetTestUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "818727",
                    Username = "alice",
                    Password = "alice",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        new Claim(JwtClaimTypes.Address,
                            @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                            IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("project:notifications:subscribe", "true", ClaimValueTypes.Boolean),
                        new Claim("project:notifications:unsubscribe", "true", ClaimValueTypes.Boolean),
                        new Claim("project:notifications:publish", "true", ClaimValueTypes.Boolean)
                    }
                },
                new TestUser
                {
                    SubjectId = "88421113",
                    Username = "bob",
                    Password = "bob",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Address,
                            @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                            IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("project:notifications:subscribe", "false", ClaimValueTypes.Boolean),
                        new Claim("project:notifications:unsubscribe", "false", ClaimValueTypes.Boolean),
                        new Claim("project:notifications:publish", "false", ClaimValueTypes.Boolean)
                    }
                }
            };
        }
    }
}
