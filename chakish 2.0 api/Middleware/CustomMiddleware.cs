using Repository.Services;

namespace WebApplication1.Middleware;


public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppWriteService appWriteService)
    {
        var endpoint = context.GetEndpoint();
        
        if (endpoint == null || !EndpointRequiresAuthorization(endpoint))
        {
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token empty");
            return;
        }

        var token = authorizationHeader.ToString().Replace("Bearer ", "").Trim();
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        var user = await appWriteService.ValidateJwt(token);
        if (user == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        context.Items["User"] = user;
        context.Request.Headers["User-Id"] = user.Id;
        await _next(context);
    }
    
    private bool EndpointRequiresAuthorization(Endpoint endpoint)
    {
        return endpoint.Metadata.Any(m => m is AuthorizeAttribute);
    }
}