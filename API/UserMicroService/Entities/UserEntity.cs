namespace UserMicroService.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Username { get; set; }
        
        public string Role { get; set; }  // F.eks. "Admin", "User", "Staff"
        
        // CPR-nummer og medicinoplysninger (krypteret via helper)
        public string CPRNumber { get; set; }
        public string MedicationDetails { get; set; }
        
    }
}