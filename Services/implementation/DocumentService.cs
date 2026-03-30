using Microsoft.AspNetCore.Identity;
using Sehha360.Data;
using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.Enums;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storageService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<DocumentService> _logger;
        private readonly IOcrService _ocrService;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".dcm" };
        private const long _maxFileSize = 20 * 1024 * 1024; // 20 MB

        public DocumentService(
            AppDbContext context,
            IFileStorageService storageService,
            UserManager<AppUser> userManager,
            ILogger<DocumentService> logger,
            IOcrService ocrService,
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _storageService = storageService;
            _userManager = userManager;
            _logger = logger;
            _ocrService = ocrService;
            _scopeFactory = scopeFactory;
        }

        public async Task<ApiResponse> UploadDocumentAsync(IFormFile file, string patientId)
        {
            if (file == null || file.Length == 0)
                return ApiResponse.FaliureResponse("No file uploaded");

            if (file.Length > _maxFileSize)
                return ApiResponse.FaliureResponse("File size exceeds 20 MB limit");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return ApiResponse.FaliureResponse("Unsupported file type. Allowed types: PDF, JPEG, PNG, DICOM");
            
            var fileName = file.FileName.Replace(" ", "_");
            var document = new MedicalDocument
            {
                FileName = fileName,
                DocumentType = GetDocumentType(extension),
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                PatientId = patientId,
                ProcessingStatus = DocumentProcessingStatus.Pending,
                FilePath = string.Empty
            };

            _context.MedicalDocuments.Add(document);
            await _context.SaveChangesAsync();

            try
            {
                using var stream = file.OpenReadStream();
                var filePath = await _storageService.UploadFileAsync(stream, fileName, file.ContentType);
                
                document.FilePath = filePath;
                document.ProcessingStatus = DocumentProcessingStatus.Processing;
                await _context.SaveChangesAsync();

                string extractedText = string.Empty;
                try
                {
                    using var ocrStream = file.OpenReadStream();
                    extractedText = await _ocrService.ExtractTextAsync(ocrStream, file.ContentType);

                    document.ExtractedText = extractedText;
                    document.ProcessingStatus = DocumentProcessingStatus.Processed;
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during OCR for document {document.Id}");
                    extractedText = $"OCR failed: {ex.Message}";
                }

                return ApiResponse.SuccessResponse("Document uploaded successfully.", new { document.Id, document.FileName, extractedText });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to storage");
                return ApiResponse.FaliureResponse("Error storing document. Please try again later.");
            }
        }

        public async Task<ApiResponse> GetDocumentUrlAsync(int documentId, string userId)
        {
            var document = await _context.MedicalDocuments.FindAsync(documentId);
            if (document == null)
                return ApiResponse.FaliureResponse("Document not found");
            if (document.PatientId != userId)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse.FaliureResponse("Unauthorized to access this document");
            }

            if (document.ProcessingStatus == DocumentProcessingStatus.MalwareDetected || 
                document.ProcessingStatus == DocumentProcessingStatus.Quarantined)
            {
                return ApiResponse.FaliureResponse("Document is quarantined due to potential security risks.");
            }

            try
            {
                var url = await _storageService.GetPreSignedUrlAsync(document.FilePath, TimeSpan.FromMinutes(15));
                
                _context.DocumentAccessLogs.Add(new DocumentAccessLog
                {
                    DocumentId = document.Id,
                    AccessedByUserId = userId,
                    AccessedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResponse("Secure URL generated successfully.", new { url, expiresAt = DateTime.UtcNow.AddMinutes(15) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating pre-signed URL for document {documentId}");
                return ApiResponse.FaliureResponse("Error generating download link. Please try again later.");
            }
        }

        private DocumentType GetDocumentType(string extension)
        {
            return extension switch
            {
                ".pdf" => DocumentType.PDF,
                ".jpg" or ".jpeg" => DocumentType.JPEG,
                ".png" => DocumentType.PNG,
                ".dcm" => DocumentType.DICOM,
                _ => DocumentType.PDF
            };
        }
    }
}
