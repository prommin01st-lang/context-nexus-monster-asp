using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DevContextNexus.API.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "x-api-key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip check for Swagger/Scalar UI to allow easy testing
            if (context.Request.Path.StartsWithSegments("/scalar") || 
                context.Request.Path.StartsWithSegments("/openapi") ||
                context.Request.Path.Value == "/")
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>("ApiKey");

            if (string.IsNullOrEmpty(apiKey) || !apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}
