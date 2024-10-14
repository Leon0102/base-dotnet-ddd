using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Database.Base;
using Shared.Domain.Constants;

namespace Domain.Users.Users.Entities
{
    public partial class User : BaseEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string MiddleName { get; set; }
        public required string Email { get; set; }
        public string? ResetToken { get; set; }
        [JsonIgnore] public string Password { get; set; }
        [JsonIgnore] public List<RefreshToken> RefreshTokens { get; set; }
        public Role Role { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public DateTime? PasswordReset { get; set; }
        public Permissions[] Permissions { get; set; }
    }
}