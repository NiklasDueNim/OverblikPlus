namespace TaskMicroService.dtos.TaskStep
{
    public class UpdateTaskStepDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Text { get; set; }
        public bool RequiresQrCodeScan { get; set; }
        public string? ImageBase64 { get; set; }
    }

}