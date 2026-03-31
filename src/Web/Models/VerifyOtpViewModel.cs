using System.ComponentModel.DataAnnotations;

namespace AuthenticationSystem.Web.Models;

public sealed class VerifyOtpViewModel
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(6)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "OTP must be numeric.")]
    public string Code { get; set; } = string.Empty;
}
