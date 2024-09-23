using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Dto;

public class ReadTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string ImageUrl { get; set; }
    
    public bool IsCompleted { get; set; }

    public DateTime DueDate { get; set; }

    public int UserId { get; set; }
    
    public string Username { get; set; }

}