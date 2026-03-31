using AuthenticationSystem.Application.DTOs;
using FluentValidation;

namespace AuthenticationSystem.Application.Validation;

public sealed class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[0-9]+$").WithMessage("OTP must be numeric.");
    }
}
