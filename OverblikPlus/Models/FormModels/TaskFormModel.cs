namespace OverblikPlus.Models.FormModels;

public class TaskFormModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageBase64 { get; set; }
    public string? UserId { get; set; }
    public string RecurrenceType { get; set; } = "None";
    public int RecurrenceInterval { get; set; } = 1;
    public DateTime StartDate { get; set; } = DateTime.Now;
    
    // Nye properties for forbedret gentagelse
    public string MonthlyType { get; set; } = "SameDay"; // SameDay, FirstDay, LastDay, SpecificDay
    public int MonthlyDay { get; set; } = 1; // For SpecificDay option
    public Dictionary<string, bool> SelectedWeekDays { get; set; } = new(); // For weekly recurrence
    public string EndType { get; set; } = "Never"; // Never, After, Date
    public int EndAfterCount { get; set; } = 1;
    public DateTime? EndDate { get; set; }
}