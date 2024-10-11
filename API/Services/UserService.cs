using Domain.Auth.Auth;
using Domain.Auth.Auth.Models;
using Domain.Auth.Auth.Models.AccountViewModels;
using Domain.Users.Users.Entities;
using Domain.Users.Users.Interface;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shared.Services.Helpers;

namespace API.Services;

public interface IUserService
{
    Task<AuthenticateResponse> Authenticate(LoginViewModel model, string ipAddress);
    Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
    Task RevokeToken(string token, string ipAddress);
    IEnumerable<User> GetAll();
    Task<User> GetById(string id);
    Task<User> Create(RegisterViewModel model);
    Task<User>  Register(RegisterViewModel model, StringValues requestHeader);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtUtils _jwtUtils;
    private readonly AppSettings _appSettings;

    public UserService(
        IUserRepository userRepository,
        IJwtUtils jwtUtils,
        IOptions<AppSettings> appSettings)
    {
        _userRepository = userRepository;
        _jwtUtils = jwtUtils;
        _appSettings = appSettings.Value;
    }

    public async Task<AuthenticateResponse> Authenticate(LoginViewModel model, string ipAddress)
    {
        var user = await _userRepository.FindByEmailAsync(model.Email);

        // validate
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            throw new AppException("Username or password is incorrect");

        // authentication successful so generate jwt and refresh tokens
        var jwtToken = _jwtUtils.GenerateJwtToken(user);
        var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        user.RefreshTokens.Add(refreshToken);

        // remove old refresh tokens from user
        RemoveOldRefreshTokens(user);

        // save changes to db
        _userRepository.Update(user);

        return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
    }

    public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
    {
        var user = await GetUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            RevokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
            _userRepository.Update(user);
        }

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);

        // remove old refresh tokens from user
        RemoveOldRefreshTokens(user);

        // save changes to db
        _userRepository.Update(user);

        // generate new jwt
        var jwtToken = _jwtUtils.GenerateJwtToken(user);

        return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
    }

    public async Task RevokeToken(string token, string ipAddress)
    {
        var user = await GetUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        // revoke token and save
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        _userRepository.Update(user);
    }

    public IEnumerable<User> GetAll()
    {
        return _userRepository.GetAll();
    }

    public async Task<User> GetById(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }

    public async Task<User> Create(RegisterViewModel model)
    {
        var user = await _userRepository.FindByEmailAsync(model.Email);
        // validate
        if (user != null)
            throw new AppException($"Email '{model.Email}' is already registered");

        // map model to new user object
        var newUser = new User()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            RefreshTokens = new List<RefreshToken>()
        };

        if(!newUser.ValidOnAdd())
            throw new AppException("Invalid user data");

        // save user
        _userRepository.AddAsync(newUser);

        return newUser;
    }

    public Task<User> Register(RegisterViewModel model, StringValues requestHeader)
    {
        // map model to new user object
        var user = new User()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            RefreshTokens = []
        };

        if (!user.ValidOnAdd())
            throw new AppException("Invalid user data");

        // save user
        _userRepository.AddAsync(user);
        return Task.FromResult(user);
    }

    // helper methods

    private async Task<User> GetUserByRefreshToken(string token)
    {
        var user = await _userRepository.GetByRefreshToken(token);

        if (user == null)
            throw new AppException("Invalid token");

        return user;
    }

    private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void RemoveOldRefreshTokens(User user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user.RefreshTokens.RemoveAll(x => 
            !x.IsActive && 
            x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
    }

    private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string? reason)
    {
        // recursively traverse the refresh token chain and ensure all descendants are revoked
        if(!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
            if (childToken is { IsActive: true })
                RevokeRefreshToken(childToken, ipAddress, reason);
            else
                RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
        }
    }

    private void RevokeRefreshToken(RefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
}