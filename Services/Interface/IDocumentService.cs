using Sehha360.Models.ApiResponse;

namespace Sehha360.Services.Interface
{
    public interface IDocumentService
    {
        Task<ApiResponse> UploadDocumentAsync(IFormFile file, string patientId);
        Task<ApiResponse> GetDocumentUrlAsync(int documentId, string userId);
        Task<ApiResponse> SummarizeDocumentAsync(int documentId, string userId);
    }
}
