using FluentValidation;
using TaskMicroService.dtos;
using TaskMicroService.dtos.Task;

namespace TaskMicroService.Validators;

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        
        RuleFor(x => x.RecurrenceInterval)
            .GreaterThan(0).WithMessage("RecurrenceInterval must be greater than zero.");

    }
}