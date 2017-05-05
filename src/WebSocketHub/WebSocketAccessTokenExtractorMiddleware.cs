using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebSocketHub
{
    /// <summary>
    /// Extracts the access token from the query string and sets it as the Authorization header.
    /// This is needed because the websocket javascript does not support custom headers, so we have to use
    /// it as the connection query string.
    /// </summary>
    public class WebSocketAccessTokenExtractorMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketAccessTokenExtractorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Query.ContainsKey("auth"))
            {
                var authToken = context.Request.Query["auth"];
                context.Request.Headers.Add("Authorization", "Bearer " + authToken);
            }
            await _next.Invoke(context);
        }
    }
}