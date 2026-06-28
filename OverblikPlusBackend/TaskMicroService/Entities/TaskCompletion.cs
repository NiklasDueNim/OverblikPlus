namespace TaskMicroService.Entities;

// Records that a single occurrence of a task (recurring or one-off) was completed.
// Keyed by (TaskId, OccurrenceDate) so each day's occurrence is tracked independently.
public class TaskCompletion
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string? UserId { get; set; }

    // The specific occurrence day that was completed (date only).
    public DateTime OccurrenceDate { get; set; }

    public DateTime CompletedAt { get; set; }
}
