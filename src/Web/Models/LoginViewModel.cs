using System.ComponentModel.DataAnnotations;

namespace AuthenticationSystem.Web.Models;

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
