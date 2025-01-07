using OverblikPlus.Models.Dtos.User;

namespace OverblikPlus.Models.Dtos.Auth;

public class LoginResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    
    public  ReadUserDto  User { get; set; }
}