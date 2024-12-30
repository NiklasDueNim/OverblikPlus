namespace TaskMicroService.dtos.TaskStep
{
    public class CreateTaskStepDto
    {
        public string? ImageBase64 { get; set; }
        
        public string Text { get; set; }
        
        public int StepNumber { get; set; }
        public int TaskId { get; set; }
        
        public bool RequiresQrCodeScan { get; set; } 
    }
}