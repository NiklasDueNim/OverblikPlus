namespace OverblikPlus;

public class TaskFormModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageBase64 { get; set; }
    public string? UserId { get; set; }
}