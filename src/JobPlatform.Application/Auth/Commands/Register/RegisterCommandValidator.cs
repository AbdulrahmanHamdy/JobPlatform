using FluentValidation;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Constants;

namespace JobPlatform.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandValidator(IIdentityService identityService)
    {
        _identityService = identityService;

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .MustAsync(async (email, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(email)) return true;
                return await _identityService.IsEmailUniqueAsync(email, cancellation);
            }).WithMessage("The specified email address is already registered.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");

        RuleFor(v => v.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required.")
            .Equal(v => v.Password).WithMessage("Password and confirmation password do not match.");

        RuleFor(v => v.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => Roles.Registerable.Contains(role))
            .WithMessage($"Role must be one of the following: {string.Join(", ", Roles.Registerable)}. Self-registration as Admin is not permitted.");
    }
}
