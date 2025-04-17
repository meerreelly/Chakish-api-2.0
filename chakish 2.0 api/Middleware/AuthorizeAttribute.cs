using Appwrite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string[] Roles { get; set; } = Array.Empty<string>();
        
        public AuthorizeAttribute() { }
        
        public AuthorizeAttribute(string roles)
        {
            Roles = roles.Split(',').Select(x => x.Trim()).ToArray();
        }
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            User user = context.HttpContext.Items["User"] as User;
            if (user == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }
            
            if (Roles.Length > 0)
            {
                bool hasRequiredRole = false;
                var userLabels = user.Labels;
                foreach (var role in Roles)
                {
                    if (userLabels.Contains(role))
                    {
                        hasRequiredRole = true;
                        break;
                    }
                }
                if (!hasRequiredRole)
                {
                    context.Result = new JsonResult(new { message = "Forbidden" })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
        }
        
    }