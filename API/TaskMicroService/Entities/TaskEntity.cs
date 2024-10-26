namespace TaskMicroService.Entities;

public class TaskEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string ImageUrl { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime DueDate { get; set; }

    public int? UserId { get; set; }
    public UserEntity? User { get; set; }

    public ICollection<TaskStep> Steps { get; set; } = new List<TaskStep>();
}