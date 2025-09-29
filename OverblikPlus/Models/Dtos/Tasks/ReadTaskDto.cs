using System.Reflection.Metadata.Ecma335;
using OpenQA.Selenium.BiDi.Modules.Script;
using OverblikPlus.Models.Dtos.TaskSteps;

namespace OverblikPlus.Models.Dtos.Tasks;

public class ReadTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Image { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }
    public List<ReadTaskStepDto> Steps { get; set; } = new List<ReadTaskStepDto>();

    public string? UserId { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public string RecurrenceType { get; set; }
    
    public int RecurrenceInterval { get; set; }
    public DateTime? NextOccurrence { get; set; }

    public DateTime StartDate { get; set; }
    
    // Nye properties for forbedret gentagelse
    public string MonthlyType { get; set; } = "SameDay";
    public int MonthlyDay { get; set; } = 1;
    public Dictionary<string, bool> SelectedWeekDays { get; set; } = new();
    public string EndType { get; set; } = "Never";
    public int EndAfterCount { get; set; } = 1;
    public DateTime? EndDate { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
    public bool HasSteps => Steps.Any();

}