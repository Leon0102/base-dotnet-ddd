using System.ComponentModel.DataAnnotations;

namespace Domain.Users.Users.Entities
{
    public partial class User 
    {
        public User(string userName
            , string email) : base()
        {
            UserName = userName;
            Email = email;
        }

        public bool ValidOnAdd()
        {
            return
                // Validate userName
                !string.IsNullOrEmpty(UserName)
                // Make sure email not null and correct email format
                && !string.IsNullOrEmpty(Email)
                && new EmailAddressAttribute().IsValid(Email);
        }
    }
}
