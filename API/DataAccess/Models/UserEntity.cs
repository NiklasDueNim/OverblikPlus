namespace DataAccess.Models;

public class UserEntity
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public string Role { get; set; }
    
    public string CPRNumber { get; set; }
    
    public string MedicationDetails { get; set; }
    
    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    
    public int XP { get; set; }
    
    public int Level { get; set; }

}