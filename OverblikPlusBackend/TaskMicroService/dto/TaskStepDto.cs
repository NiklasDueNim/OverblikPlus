namespace TaskMicroService.dto;

public class TaskStepDto
{
    public int? Id { get; set; }
    
    public string? Image { get; set; }
    
    public string Text { get; set; }
    
    public int StepNumber { get; set; }
}