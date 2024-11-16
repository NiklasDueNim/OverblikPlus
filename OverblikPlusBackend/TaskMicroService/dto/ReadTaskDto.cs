namespace TaskMicroService.dto;

public class ReadTaskDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    public List<ReadTaskStepDto>? Steps { get; set; } = new();
    public string? Image { get; set; }
    public bool RequiresQrCodeScan { get; set; }

    public string UserId { get; set; }
}