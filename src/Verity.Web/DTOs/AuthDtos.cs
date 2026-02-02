using System.ComponentModel.DataAnnotations;

namespace Verity.Web.DTOs;

public class LoginModel
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class RegisterModel
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] [MinLength(6)] public string Password { get; set; } = string.Empty;
    [Required] [Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
