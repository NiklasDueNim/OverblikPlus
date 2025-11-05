namespace TaskMicroService.Services.Recurrence;

public class RecurrenceOptions
{
    public DateTime StartDate { get; set; }
    public string RecurrenceType { get; set; } = "None";
    public int RecurrenceInterval { get; set; } = 1;
    public string MonthlyType { get; set; } = "SameDay";
    public int MonthlyDay { get; set; } = 1;
    public Dictionary<string, bool>? SelectedWeekDays { get; set; }
    public string EndType { get; set; } = "Never";
    public int EndAfterCount { get; set; } = 1;
    public DateTime? EndDate { get; set; }
}

