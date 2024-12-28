using FluentValidation;
using UserMicroService.dto;

namespace UserMicroService.Validators;

public class ReadUserDtoValidator : AbstractValidator<ReadUserDto>
{
    public ReadUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role == "Admin" || role == "Staff" || role == "User")
            .WithMessage("Invalid role specified.");
    }
}