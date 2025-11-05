using OverblikPlus.Shared.Interfaces;

namespace TaskMicroService.Services.Recurrence;

public class RecurrenceCalculator : IRecurrenceCalculator
{
    private readonly ILoggerService _logger;

    public RecurrenceCalculator(ILoggerService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public DateTime CalculateNext(DateTime startDate, RecurrenceOptions options)
    {
        if (options.RecurrenceType == "None") 
            return startDate;

        var currentDate = startDate;
        var occurrenceCount = 0;

        while (occurrenceCount < 100) // Safety limit
        {
            currentDate = options.RecurrenceType switch
            {
                "Daily" => currentDate.AddDays(options.RecurrenceInterval),
                "Weekly" => GetNextWeeklyOccurrence(currentDate, options.RecurrenceInterval, options.SelectedWeekDays),
                "Monthly" => CalculateMonthlyOccurrence(currentDate, options.RecurrenceInterval, options.MonthlyType, options.MonthlyDay),
                "Yearly" => currentDate.AddYears(options.RecurrenceInterval),
                _ => throw new ArgumentException("Invalid recurrence type")
            };

            occurrenceCount++;

            // Check end conditions
            if (options.EndType == "After" && occurrenceCount >= options.EndAfterCount) 
                break;
            if (options.EndType == "Date" && options.EndDate.HasValue && currentDate > options.EndDate.Value) 
                break;

            // For weekly, check if we have a valid day
            if (options.RecurrenceType == "Weekly" && IsValidWeekDay(currentDate, options.SelectedWeekDays))
            {
                return currentDate;
            }
            else if (options.RecurrenceType != "Weekly")
            {
                return currentDate;
            }
        }

        return currentDate;
    }

    public bool IsValidWeekDay(DateTime date, Dictionary<string, bool>? selectedWeekDays)
    {
        if (selectedWeekDays == null || !selectedWeekDays.Any()) 
            return true;

        var dayName = GetDanishDayName(date.DayOfWeek);
        return selectedWeekDays.ContainsKey(dayName) && selectedWeekDays[dayName];
    }

    public string GetDanishDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Mandag",
            DayOfWeek.Tuesday => "Tirsdag",
            DayOfWeek.Wednesday => "Onsdag",
            DayOfWeek.Thursday => "Torsdag",
            DayOfWeek.Friday => "Fredag",
            DayOfWeek.Saturday => "Lørdag",
            DayOfWeek.Sunday => "Søndag",
            _ => ""
        };
    }

    private DateTime CalculateMonthlyOccurrence(DateTime startDate, int interval, string monthlyType, int monthlyDay)
    {
        var nextDate = startDate.AddMonths(interval);

        return monthlyType switch
        {
            "SameDay" => nextDate, // Beholder samme dag (kan være problematisk)
            "FirstDay" => new DateTime(nextDate.Year, nextDate.Month, 1),
            "LastDay" => new DateTime(nextDate.Year, nextDate.Month, 1).AddMonths(1).AddDays(-1),
            "SpecificDay" => GetSpecificDayInMonth(nextDate, monthlyDay),
            _ => nextDate
        };
    }

    private DateTime GetSpecificDayInMonth(DateTime month, int day)
    {
        // Sikrer at vi ikke overskrider månedens antal dage 
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        var targetDay = Math.Min(day, daysInMonth);

        return new DateTime(month.Year, month.Month, targetDay);
    }

    private DateTime GetNextWeeklyOccurrence(DateTime currentDate, int interval, Dictionary<string, bool>? selectedWeekDays)
    {
        var nextDate = currentDate.AddDays(7 * interval);

        // Find next valid weekday
        for (int i = 0; i < 7; i++)
        {
            var dayName = GetDanishDayName(nextDate.DayOfWeek);
            if (selectedWeekDays?.ContainsKey(dayName) == true && selectedWeekDays[dayName])
            {
                return nextDate;
            }

            nextDate = nextDate.AddDays(1);
        }

        return nextDate;
    }
}

