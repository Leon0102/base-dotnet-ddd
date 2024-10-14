using System.ComponentModel.DataAnnotations;

namespace Domain.Auth.Auth.Models.AccountViewModels;

public class ValidateResetTokenViewModel
{
    [Required]
    public string Token { get; set; }
}