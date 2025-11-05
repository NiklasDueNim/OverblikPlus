namespace TaskMicroService.Entities;

public class TaskEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; } 
    public bool IsCompleted { get; set; }
    
    public string RecurrenceType { get; set; }
    public int RecurrenceInterval { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? NextOccurrence { get; set; } 
    public string? UserId { get; set; }
    public bool RequiresQrCodeScan { get; set; }
    
    // SeriesId: Alle occurrences af samme gentagende opgave deler samme SeriesId
    // For den første opgave i serien, er SeriesId = Id (eller null hvis ikke sat endnu)
    public int? SeriesId { get; set; }
    
    // Nye properties for forbedret gentagelse
    public string MonthlyType { get; set; } = "SameDay";
    public int MonthlyDay { get; set; } = 1;
    public string SelectedWeekDays { get; set; } = "{}"; // JSON serialized Dictionary<string, bool>
    public string EndType { get; set; } = "Never";
    public int EndAfterCount { get; set; } = 1;
    public DateTime? EndDate { get; set; }
    
    public ICollection<TaskStep>? Steps { get; set; } = new List<TaskStep>();
}