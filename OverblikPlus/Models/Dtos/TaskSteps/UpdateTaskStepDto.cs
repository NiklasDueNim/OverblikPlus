namespace OverblikPlus.Models.Dtos.TaskSteps;

public class UpdateTaskStepDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }

    public int StepNumber { get; set; }
    
    public string Text { get; set; }
    
    public string? ImageBase64 { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }
    
}