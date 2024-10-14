using Shared.Domain.Constants;

namespace Domain.Auth.Auth;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute
{
    public Permissions[] Permissions { get; } // Accept multiple permissions

    public HasPermissionAttribute(params Permissions[] permissions) // 'params' allows passing multiple arguments
    {
        Permissions = permissions;
    }
}
