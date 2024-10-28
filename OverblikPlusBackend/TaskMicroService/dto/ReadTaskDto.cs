namespace TaskMicroService.dto;

public class ReadTaskDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    public List<TaskStepDto>? Steps { get; set; } = new();
    
    public string? Image { get; set; }

}