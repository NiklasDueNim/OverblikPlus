namespace UserMicroService;

public class AuthResult
{
    public bool Success { get; set; }

    public IEnumerable<string> Errors { get; set; } = new List<string>();

}