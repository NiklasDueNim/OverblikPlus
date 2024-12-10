namespace OverblikPlus;

public class TaskStepFormModel
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string Text { get; set; }
    public string ImageBase64 { get; set; }
    
    public int StepNumber { get; set; }
    public bool RequiresQrCodeScan { get; set; }
}