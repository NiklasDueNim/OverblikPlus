namespace OverblikPlus.Dtos.TaskSteps;

public class UpdateTaskStepDto
{
    public string Text { get; set; }
    public string? ImageBase64 { get; set; }
    public bool RequiresQrCodeScan { get; set; }
    
}