namespace UserMicroService.dto;

public class LoginResponseDto
{
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public  ReadUserDto  User { get; set; }
}
