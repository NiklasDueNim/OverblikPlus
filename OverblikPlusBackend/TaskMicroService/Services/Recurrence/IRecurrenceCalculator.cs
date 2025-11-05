namespace TaskMicroService.Services.Recurrence;

public interface IRecurrenceCalculator
{
    DateTime CalculateNext(DateTime startDate, RecurrenceOptions options);
    bool IsValidWeekDay(DateTime date, Dictionary<string, bool>? selectedWeekDays);
    string GetDanishDayName(DayOfWeek dayOfWeek);
}

