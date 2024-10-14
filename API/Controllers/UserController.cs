using API.Services;
using Domain.Auth.Auth;
using Domain.Auth.Auth.Models.AccountViewModels;
using Domain.Users.Users.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Constants;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Authenticate(LoginViewModel model)
    {
        var response = await _userService.Authenticate(model, ipAddress());
        setTokenCookie(response.RefreshToken);
        return Ok(response);
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateAccountViewModel model)
    {
        var user = await _userService.Register(model, Request.Headers["origin"]);
        return Ok(new { message = "Registration successful, please check your email for verification instructions" });
    }
    
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var user = HttpContext.Items["User"]!;
        return Ok(user);
    }
    

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var response =  await _userService.RefreshToken(refreshToken, ipAddress());
        setTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpPost("revoke-token")]
    public IActionResult RevokeToken(RevokeTokenModel model)
    {
        // accept refresh token in request body or cookie
        var token = model.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token is required" });

        _userService.RevokeToken(token, ipAddress());
        return Ok(new { message = "Token revoked" });
    }
    
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, UpdateAccountViewModel model)
    {
        var user = (User)HttpContext.Items["User"];
        if (id != user.Id.ToString() && user.Role != Role.Admin)
            return Unauthorized(new { message = "Unauthorized" });
        // only admins can update role
        if (user.Role != Role.Admin)
            model.Role = user.Role;
        user = await _userService.Update(id, model);
        return Ok(user);
    }
    
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        _userService.ForgotPassword(model, Request.Headers["origin"]);
        return Ok(new { message = "Please check your email for password reset instructions" });
    }
    
    [HttpPost("validate-reset-token")]
    public IActionResult ValidateResetToken(ValidateResetTokenViewModel model)
    {
        _userService.ValidateResetToken(model);
        return Ok(new { message = "Token is valid" });
    }
    
    [HttpPost("reset-password")]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        _userService.ResetPassword(model);
        return Ok(new { message = "Password reset successful, you can now login" });
    }

    [HttpGet]
    [Authorize([Role.Admin, Role.FM_Admin])]
    [HasPermission(Permissions.Users_View)]
    public IActionResult GetAll()
    {
        var users = _userService.GetAll();
        return Ok(users);
    }

    
    [HttpGet("{id}")]
    [Authorize([Role.Admin, Role.FM_Admin])]
    [HasPermission(Permissions.Users_View)]
    public IActionResult GetById(string id)
    {
        var user = _userService.GetById(id);
        return Ok(user);
    }

    [HttpGet("{id}/refresh-tokens")]
    public async Task<IActionResult> GetRefreshTokens(string id)
    {
        var user = await _userService.GetById(id);
        return Ok(user.RefreshTokens);
    }

    // helper methods

    private void setTokenCookie(string token)
    {
        // append cookie with refresh token to the http response
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string ipAddress()
    {
        // get source ip address for the current request
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }
}