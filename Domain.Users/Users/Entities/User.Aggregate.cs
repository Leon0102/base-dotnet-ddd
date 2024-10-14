using System.ComponentModel.DataAnnotations;
using Domain.Users.Users.Interface;
using Shared.Domain.Constants;

namespace Domain.Users.Users.Entities
{
    public partial class User
    {
        public User(string firstName, string lastName, string middleName, string email, string password,
            Role role, List<RefreshToken> refreshTokens,
            string resetToken = null)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            Email = email;
            Password = HashPassword(password);
            Role = role;
            RefreshTokens = refreshTokens;
            ResetToken = resetToken;
        }

        public User()
        {
            
        }

        public bool ValidOnAdd()
        {
            return
                // Validate userName
                !string.IsNullOrEmpty(FirstName)
                && !string.IsNullOrEmpty(LastName)
                && !string.IsNullOrEmpty(MiddleName)
                // Make sure email not null and correct email format
                && !string.IsNullOrEmpty(Email)
                && new EmailAddressAttribute().IsValid(Email);
        }
        
        // Method to hash password
        private string HashPassword(string password)
        {
            // Use BCrypt to hash the password with a salt
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Method to verify the password against the hashed password
        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, Password);
        }
    }
}
