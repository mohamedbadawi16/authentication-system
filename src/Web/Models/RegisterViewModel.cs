using System.ComponentModel.DataAnnotations;

namespace AuthenticationSystem.Web.Models;

public sealed class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$", ErrorMessage = "Password must contain upper, lower case letters and a number.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
