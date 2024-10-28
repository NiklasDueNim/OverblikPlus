namespace TaskMicroService.Services
{
    public interface IImageConversionService
    {
        string ConvertToBase64(byte[] byteArray);
    }
}