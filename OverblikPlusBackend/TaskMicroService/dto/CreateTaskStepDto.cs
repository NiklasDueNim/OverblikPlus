namespace TaskMicroService.dto
{
    public class CreateTaskStepDto
    {
        public string? ImageBase64 { get; set; }
        
        public string Text { get; set; }
        
        public int StepNumber { get; set; }
        public int TaskId { get; set; }  // For at linke til den overordnede Task
    }
}