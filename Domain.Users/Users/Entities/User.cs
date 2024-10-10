using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Database.Base;

namespace Domain.Users.Users.Entities
{
    [Table("Users")]
    public partial class User : DeleteEntity<int>
    {
        public User()
        {
            
        }

        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public short DepartmentId { get; set; }
    }
}