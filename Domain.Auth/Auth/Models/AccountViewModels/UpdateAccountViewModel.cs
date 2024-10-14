using System.ComponentModel.DataAnnotations;
using Domain.Users.Users.Entities;
using Shared.Domain.Constants;

namespace Domain.Auth.Auth.Models.AccountViewModels;

public class UpdateAccountViewModel
{
    private string _password;
    private string _confirmPassword;
    private string _role;
    private string _email;
        
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }

    [EnumDataType(typeof(Role))]
    public Role Role
    {
        get => Enum.Parse<Role>(_role);
        set => _role = value.ToString();
    }

    [EmailAddress]
    public string Email
    {
        get => _email;
        set => _email = replaceEmptyWithNull(value);
    }

    [MinLength(6)]
    public string Password
    {
        get => _password;
        set => _password = replaceEmptyWithNull(value);
    }

    [Compare("Password")]
    public string ConfirmPassword 
    {
        get => _confirmPassword;
        set => _confirmPassword = replaceEmptyWithNull(value);
    }

    // helpers

    private string replaceEmptyWithNull(string value)
    {
        // replace empty string with null to make field optional
        return string.IsNullOrEmpty(value) ? null : value;
    }
}