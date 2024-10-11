using System.ComponentModel.DataAnnotations;

namespace Domain.Auth.Auth.Models.AccountViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}