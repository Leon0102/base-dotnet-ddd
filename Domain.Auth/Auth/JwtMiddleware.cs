using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Domain.Users.Users.Interface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Infra;
using Shared.Services.Helpers;

namespace Domain.Auth.Auth;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
    {
        _next = next;
        _appSettings = appSettings.Value;
    }

    public async Task Invoke(HttpContext context, IUserRepository userRepository, IJwtUtils jwtUtils)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var userId = jwtUtils.ValidateJwtToken(token);
        if (userId != null)
        {
            await attachUserToContext(context, userRepository, userId);
        }

        await _next(context);
    }
    
    private async Task attachUserToContext(HttpContext context, IUserRepository userRepository, string userId)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            context.Items["User"] = await userRepository.GetById(userId);
        }
        catch 
        {
            // do nothing if jwt validation fails
            // account is not attached to context so request won't have access to secure routes
        }
    }
}