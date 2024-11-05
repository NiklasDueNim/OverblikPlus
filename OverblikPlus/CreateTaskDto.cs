namespace OverblikPlus;

public class CreateTaskDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string ImageBase64 { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }

    public int UserId { get; set; }
}