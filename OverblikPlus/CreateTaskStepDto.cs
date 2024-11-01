namespace OverblikPlus;

public class CreateTaskStepDto
{
    public int TaskId { get; set; }
    
    public string Text { get; set; }
    
    public string ImageBase64 { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }
}