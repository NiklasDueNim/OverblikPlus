namespace TaskMicroService.Entities;

public class TaskStep
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int StepNumber { get; set; }
    
    public string? ImageUrl { get; set; } = String.Empty;
    
    public string Text { get; set; } = string.Empty;

    public bool RequiresQrCodeScan { get; set; }
    
    public int? NextStepId { get; set; }
    public TaskEntity Task { get; set; }
}