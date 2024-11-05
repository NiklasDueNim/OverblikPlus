namespace OverblikPlus;

public class ReadTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Image { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }
    public List<TaskStepDto> Steps { get; set; } = new List<TaskStepDto>();

    public int UserId { get; set; }
    
    public bool IsCompleted { get; set; }

}