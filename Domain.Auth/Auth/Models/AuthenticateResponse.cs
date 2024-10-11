using System.Text.Json.Serialization;
using Domain.Users.Users.Entities;

namespace Domain.Auth.Auth.Models;

public class AuthenticateResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string JwtToken { get; set; }

    [JsonIgnore] // refresh token is returned in http only cookie
    public string? RefreshToken { get; set; }

    public AuthenticateResponse(User user, string jwtToken, string? refreshToken)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Username = user.MiddleName;
        JwtToken = jwtToken;
        RefreshToken = refreshToken;
    }
}