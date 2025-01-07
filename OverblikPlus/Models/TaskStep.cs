namespace OverblikPlus.Models;

public class TaskStep
{
    public int Id { get; set; }
    
    public int TaskId { get; set; }

    public int StepNumber { get; set; }
    
    public string Image { get; set; }
    
    public string Text { get; set; } 
    
    public bool RequiresQrCodeScan { get; set; }

    public int? NextStepId { get; set; }

}