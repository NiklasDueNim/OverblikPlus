namespace API.Dto;

public class TaskDto
{
    public int Id { get; set; } // Brugeren kender vel ikke id, Description, ImageUrl
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string ImageUrl { get; set; }
    
    public bool IsCompleted { get; set; }

}