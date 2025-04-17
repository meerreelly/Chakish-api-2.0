using Microsoft.AspNetCore.Http;

namespace WebApplication1.Middleware;

public static class HttpContextExtensions
{
    public static T GetUser<T>(this HttpContext context) where T : class
    {
        context.Items.TryGetValue("User", out var user);
        return user as T;
    }
}