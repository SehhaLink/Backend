using Supabase;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class SupabaseFileStorageService : IFileStorageService
    {
        private readonly Client _supabase;
        private readonly string _bucketName;

        public SupabaseFileStorageService(Client supabase, IConfiguration configuration)
        {
            _supabase = supabase;
            _bucketName = Environment.GetEnvironmentVariable("SUPABASE_BUCKET") ?? configuration["Supabase:Bucket"] ?? "medical-documents";
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var key = $"{Guid.NewGuid()}-{fileName}";
            
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var data = memoryStream.ToArray();

            await _supabase.Storage
                .From(_bucketName)
                .Upload(data, key, new Supabase.Storage.FileOptions { ContentType = contentType });

            return key;
        }

        public async Task<string> GetPreSignedUrlAsync(string filePath, TimeSpan expiration)
        {
            var seconds = (int)expiration.TotalSeconds;
            return await _supabase.Storage
                .From(_bucketName)
                .CreateSignedUrl(filePath, seconds);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            await _supabase.Storage
                .From(_bucketName)
                .Remove(new List<string> { filePath });
        }
    }
}
