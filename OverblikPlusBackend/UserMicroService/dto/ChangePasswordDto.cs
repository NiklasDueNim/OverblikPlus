namespace UserMicroService.dto;

public class ChangePasswordDto
{
    public string Email { get; set; }
    
    public string CurrentPassword { get; set; }
    
    public string NewPassword { get; set; }
}