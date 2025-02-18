using FluentValidation;
using TaskMicroService.Dtos.Calendar;

namespace TaskMicroService.Validators.Calendar;

public class CreateCalendarEventDtoValidator : AbstractValidator<CreateCalendarEventDto>
{
    public CreateCalendarEventDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Titel er påkrævet.")
            .MaximumLength(100).WithMessage("Titel må højst være 100 tegn.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Beskrivelse må højst være 500 tegn.");

        RuleFor(x => x.StartDateTime)
            .LessThan(x => x.EndDateTime).WithMessage("Startdato skal være før slutdato.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Bruger-ID er påkrævet.");
    }
}