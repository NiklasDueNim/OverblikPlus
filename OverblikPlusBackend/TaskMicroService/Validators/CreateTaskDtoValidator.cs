using FluentValidation;
using TaskMicroService.dto;

namespace TaskMicroService.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        
        RuleFor(x => x.RecurrenceInterval)
            .GreaterThan(0).WithMessage("RecurrenceInterval must be greater than zero.");

    }
}