namespace TaskMicroService.Entities
{
    public class TaskStep
    {
        public int Id { get; set; }
        public int TaskId { get; set; } // Fremmednøgle til Task
        public int StepNumber { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        // Navigationsejendom til Task
        public TaskEntity Task { get; set; }
    }
}