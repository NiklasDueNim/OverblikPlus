using FluentValidation.TestHelper;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.Validators.Calendar;
using Xunit;

namespace TaskMicroService.Tests.UnitTests
{
    public class CreateCalendarEventDtoValidatorTests
    {
        private readonly CreateCalendarEventDtoValidator _validator;

        public CreateCalendarEventDtoValidatorTests()
        {
            _validator = new CreateCalendarEventDtoValidator();
        }

        [Fact]
        public void Validator_Should_Have_Validation_Error_When_Title_Is_Empty()
        {
            var calendarEventDto = new CreateCalendarEventDto
            {
                Title = "",
                Description = "Description",
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddDays(1),
                UserId = "user123"
            };

            var result = _validator.TestValidate(calendarEventDto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage("Titel er påkrævet.");
        }

        [Fact]
        public void Validator_Should_Have_Validation_Error_When_Title_Exceeds_Max_Length()
        {
            var calendarEventDto = new CreateCalendarEventDto
            {
                Title = new string('A', 101),
                Description = "Description",
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddDays(1),
                UserId = "user123"
            };

            var result = _validator.TestValidate(calendarEventDto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage("Titel må højst være 100 tegn.");
        }

        [Fact]
        public void Validator_Should_Have_Validation_Error_When_Description_Exceeds_Max_Length()
        {
            var calendarEventDto = new CreateCalendarEventDto
            {
                Title = "Title",
                Description = new string('A', 501),
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddDays(1),
                UserId = "user123"
            };

            var result = _validator.TestValidate(calendarEventDto);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("Beskrivelse må højst være 500 tegn.");
        }

        [Fact]
        public void Validator_Should_Have_Validation_Error_When_StartDate_Is_After_EndDate()
        {
            var calendarEventDto = new CreateCalendarEventDto
            {
                Title = "Title",
                Description = "Description",
                StartDateTime = DateTime.Now.AddDays(1),
                EndDateTime = DateTime.Now,
                UserId = "user123"
            };

            var result = _validator.TestValidate(calendarEventDto);

            result.ShouldHaveValidationErrorFor(x => x.StartDateTime)
                .WithErrorMessage("Startdato skal være før slutdato.");
        }

        [Fact]
        public void Validator_Should_Have_Validation_Error_When_UserId_Is_Empty()
        {
            var calendarEventDto = new CreateCalendarEventDto
            {
                Title = "Title",
                Description = "Description",
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddDays(1),
                UserId = ""
            };

            var result = _validator.TestValidate(calendarEventDto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage("Bruger-ID er påkrævet.");
        }

        [Fact]
        public void Validation_Result_Should_Be_Valid()
        {
            var calendarEventDto = new CreateCalendarEventDto
            {
                Title = "Title",
                Description = "Description",
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddDays(1),
                UserId = "user123"
            };

            var result = _validator.TestValidate(calendarEventDto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}