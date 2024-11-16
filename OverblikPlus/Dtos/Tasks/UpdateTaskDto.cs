namespace OverblikPlus.Dtos.Tasks;

public class UpdateTaskDto
{ 
        public int Id { get; set; }
        
        public string Name { get; set; } 
        
        public string Description { get; set; }   
        
        public string? ImageBase64 { get; set; }  
        
        public bool RequiresQrCodeScan { get; set; } 
        
        public string? UserId { get; set; }
}