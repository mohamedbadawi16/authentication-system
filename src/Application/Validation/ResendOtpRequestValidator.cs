using AuthenticationSystem.Application.DTOs;
using FluentValidation;

namespace AuthenticationSystem.Application.Validation;

public sealed class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
{
    public ResendOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
