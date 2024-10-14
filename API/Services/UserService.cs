using System.Security.Cryptography;
using Domain.Auth.Auth;
using Domain.Auth.Auth.Models;
using Domain.Auth.Auth.Models.AccountViewModels;
using Domain.Users.Users.Entities;
using Domain.Users.Users.Interface;
using EmailSender.Interface;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shared.Domain.Constants;
using Shared.Services.Helpers;

namespace API.Services;

public interface IUserService
{
    Task<AuthenticateResponse> Authenticate(LoginViewModel model, string ipAddress);
    Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
    Task RevokeToken(string token, string ipAddress);
    IEnumerable<User> GetAll();
    Task<User> GetById(string id);
    Task<User> Create(CreateAccountViewModel model);
    Task<User>  Register(CreateAccountViewModel model, StringValues requestHeader);
    Task<User> Update(string id, UpdateAccountViewModel model);
    void VerifyEmail(string token);
    Task ForgotPassword(ForgotPasswordViewModel model, string origin);
    void ValidateResetToken(ValidateResetTokenViewModel model);
    Task ResetPassword(ResetPasswordViewModel model);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtUtils _jwtUtils;
    private readonly AppSettings _appSettings;
    private readonly IEmailService _emailService;

    public UserService(
        IUserRepository userRepository,
        IJwtUtils jwtUtils,
        IOptions<AppSettings> appSettings,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _jwtUtils = jwtUtils;
        _appSettings = appSettings.Value;
        _emailService = emailService;
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

    public async Task<User> Create(CreateAccountViewModel model)
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

    public Task<User> Register(CreateAccountViewModel model, StringValues requestHeader)
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

    public Task<User> Update(string id, UpdateAccountViewModel model)
    {
        var user = _userRepository.GetByIdAsync(id).Result;

        if (user == null)
            throw new KeyNotFoundException("User not found");

        // only admins can update role
        if (user.Role != Role.Admin)
            model.Role = user.Role;

        // copy model to user and save
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.MiddleName = model.MiddleName;
        user.Email = model.Email;
        user.Role = model.Role;
        if (!string.IsNullOrEmpty(model.Password))
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

        if (!user.ValidOnAdd())
            throw new AppException("Invalid user data");

        _userRepository.Update(user);

        return Task.FromResult(user);        
    }

    public void VerifyEmail(string token)
    {
        throw new NotImplementedException();
    }

    public async Task ForgotPassword(ForgotPasswordViewModel model, string origin)
    {
        var account = await _userRepository.FindByEmailAsync(model.Email);

        // always return ok response to prevent email enumeration
        if (account == null) return;

        // create reset token that expires after 1 day
        account.ResetToken = randomTokenString();
        account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

        _userRepository.Update(account);

        // send email
        sendPasswordResetEmail(account, origin);
    }

    public void ValidateResetToken(ValidateResetTokenViewModel model)
    {
        var account = _userRepository.GetByResetToken(model.Token);

        if (account == null)
            throw new AppException("Invalid token");
    }

    public async Task ResetPassword(ResetPasswordViewModel model)
    {
        var account = await _userRepository.GetByResetToken(model.Token);

        if (account == null)
            throw new AppException("Invalid token");

        // update password and remove reset token
        account.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
        account.ResetToken = null;

        _userRepository.Update(account);
    }


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
    
    private string randomTokenString()
    {
        using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        var randomBytes = new byte[40];
        rngCryptoServiceProvider.GetBytes(randomBytes);
        // convert random bytes to hex string
        return BitConverter.ToString(randomBytes).Replace("-", "");
    }
    
    // private void sendVerificationEmail(User account, string origin)
    //     {
    //         string message;
    //         if (!string.IsNullOrEmpty(origin))
    //         {
    //             var verifyUrl = $"{origin}/account/verify-email?token={account.VerificationToken}";
    //             message = $@"<p>Please click the below link to verify your email address:</p>
    //                          <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
    //         }
    //         else
    //         {
    //             message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
    //                          <p><code>{account.VerificationToken}</code></p>";
    //         }
    //
    //         _emailService.Send(
    //             to: account.Email,
    //             subject: "Sign-up Verification API - Verify Email",
    //             html: $@"<h4>Verify Email</h4>
    //                      <p>Thanks for registering!</p>
    //                      {message}"
    //         );
    //     }
    //
    //     private void sendAlreadyRegisteredEmail(string email, string origin)
    //     {
    //         string message;
    //         if (!string.IsNullOrEmpty(origin))
    //             message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
    //         else
    //             message = "<p>If you don't know your password you can reset it via the <code>/accounts/forgot-password</code> api route.</p>";
    //
    //         _emailService.Send(
    //             to: email,
    //             subject: "Sign-up Verification API - Email Already Registered",
    //             html: $@"<h4>Email Already Registered</h4>
    //                      <p>Your email <strong>{email}</strong> is already registered.</p>
    //                      {message}"
    //         );
    //     }

        private void sendPasswordResetEmail(User account, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/account/reset-password?token={account.ResetToken}";
                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                             <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                             <p><code>{account.ResetToken}</code></p>";
            }

            _emailService.Send(
                to: account.Email,
                subject: "Sign-up Verification API - Reset Password",
                html: $@"<h4>Reset Password Email</h4>
                         {message}"
            );
        }
}