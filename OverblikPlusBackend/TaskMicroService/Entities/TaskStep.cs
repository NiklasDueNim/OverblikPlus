namespace TaskMicroService.Entities;

public class TaskStep
{
    public int Id { get; set; }
    public int TaskId { get; set; } // Fremmednøgle til Task
    public int StepNumber { get; set; }
    
    public byte[]? ImageUrl { get; set; } = Array.Empty<byte>();
    
    public string Text { get; set; } = string.Empty;

    // Navigationsejendom til Task
    public TaskEntity Task { get; set; }
}