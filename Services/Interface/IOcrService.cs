namespace Sehha360.Services.Interface
{
    public interface IOcrService
    {
        Task<string> ExtractTextAsync(Stream fileStream, string mimeType);
    }
}
