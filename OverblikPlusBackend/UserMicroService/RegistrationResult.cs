namespace UserMicroService;

public class RegistrationResult
{
    public bool Success { get; set; }
    
    public IEnumerable<string> Errors { get; set; }
}