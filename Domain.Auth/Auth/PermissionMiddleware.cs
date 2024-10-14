using Domain.Users.Users.Entities;
using Shared.Domain.Constants;

namespace Domain.Auth.Auth;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var hasPermissionAttributes = endpoint.Metadata.GetOrderedMetadata<HasPermissionAttribute>();
            if (hasPermissionAttributes.Any())
            {
                var user = context.Items["User"] as User; // Assuming user is added to context earlier

                if (user == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
                
                foreach (var attribute in hasPermissionAttributes)
                {
                    // Check if the user has all the required permissions
                    var hasAllPermissions = attribute.Permissions.All(permission =>
                        user.Permissions.Contains(permission)); // Specify the type argument

                    if (!hasAllPermissions)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Forbidden: You do not have all the required permissions.");
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}