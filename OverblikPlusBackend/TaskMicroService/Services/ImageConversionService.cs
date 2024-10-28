namespace TaskMicroService.Services
{
    public class ImageConversionService : IImageConversionService
    {
        public string ConvertToBase64(byte[] byteArray)
        {
            return "data:image/png;base64," + Convert.ToBase64String(byteArray);
        }
    }

}