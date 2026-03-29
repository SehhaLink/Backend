namespace Sehha360.Services.Interface
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<string> GetPreSignedUrlAsync(string filePath, TimeSpan expiration);
        Task DeleteFileAsync(string filePath);
    }
}
