namespace UserMicroService.dto
{
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        
        public string? Medication { get; set; }

        public string Role { get; set; }
        
        
        public string Password { get; set; }
        
        public string Email { get; set; }
    }
}