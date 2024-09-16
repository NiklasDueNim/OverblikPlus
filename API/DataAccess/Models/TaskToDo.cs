namespace DataAccess.Models;

public class TaskToDo
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string ImageUrl { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public DateTime DueDate { get; set; }

}