using System.ComponentModel.DataAnnotations;

namespace Domain.Auth.Auth.Models.AccountViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}