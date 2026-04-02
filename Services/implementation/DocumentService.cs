using Microsoft.AspNetCore.Identity;
using Sehha360.Data;
using Sehha360.Models;
using Sehha360.Models.ApiResponse;
using Sehha360.Models.Enums;
using Sehha360.Repositories.Interface;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _storageService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<DocumentService> _logger;
        private readonly IOcrService _ocrService;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".dcm" };
        private const long _maxFileSize = 20 * 1024 * 1024; // 20 MB

        public DocumentService(
            IUnitOfWork unitOfWork,
            IFileStorageService storageService,
            UserManager<AppUser> userManager,
            ILogger<DocumentService> logger,
            IOcrService ocrService,
            IServiceScopeFactory scopeFactory)
        {
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.MedicalDocuments.AddAsync(document);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                using var stream = file.OpenReadStream();
                var filePath = await _storageService.UploadFileAsync(stream, fileName, file.ContentType);
                
                document.FilePath = filePath;
                document.ProcessingStatus = DocumentProcessingStatus.Processing;
                await _unitOfWork.SaveChangesAsync();

                var docId = document.Id;
                
                // Read file to memory stream so it's not disposed when request ends
                var backgroundMemoryStream = new MemoryStream();
                await file.CopyToAsync(backgroundMemoryStream);
                var contentType = file.ContentType;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var ocrService = scope.ServiceProvider.GetRequiredService<IOcrService>();
                        var summaryService = scope.ServiceProvider.GetRequiredService<ILlmSummaryService>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<DocumentService>>();

                        // Reset stream position before reading
                        backgroundMemoryStream.Position = 0;
                        var extractedText = await ocrService.ExtractTextAsync(backgroundMemoryStream, contentType);

                        var doc = await uow.MedicalDocuments.GetByIdAsync(docId);
                        if (doc != null)
                        {
                            doc.ExtractedText = extractedText;
                            doc.ProcessingStatus = DocumentProcessingStatus.Processed;
                            await uow.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        using var errorScope = _scopeFactory.CreateScope();
                        var errorLogger = errorScope.ServiceProvider.GetRequiredService<ILogger<DocumentService>>();
                        errorLogger.LogError(ex, "Error during background processing for document {DocId}", docId);
                    }
                    finally
                    {
                        backgroundMemoryStream.Dispose();
                    }
                });

                return ApiResponse.SuccessResponse("Document uploaded successfully and is being processed in the background. Check back shortly for the summary.", new { document.Id, document.FileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to storage");
                return ApiResponse.FaliureResponse("Error storing document. Please try again later.");
            }
        }

        public async Task<ApiResponse> GetDocumentUrlAsync(int documentId, string userId)
        {
            var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(documentId);
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
                
                await _unitOfWork.DocumentAccessLogs.AddAsync(new DocumentAccessLog
                {
                    DocumentId = document.Id,
                    AccessedByUserId = userId,
                    AccessedAt = DateTime.UtcNow
                });
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse.SuccessResponse("Secure URL generated successfully.", new { url, expiresAt = DateTime.UtcNow.AddMinutes(15) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating pre-signed URL for document {documentId}");
                return ApiResponse.FaliureResponse("Error generating download link. Please try again later.");
            }
        }

        public async Task<ApiResponse> SummarizeDocumentAsync(int documentId, string userId)
        {
            var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(documentId);
            if (document == null)
                return ApiResponse.FaliureResponse("Document not found");

            if (document.PatientId != userId)
                return ApiResponse.FaliureResponse("Unauthorized to summarize this document");

            if (string.IsNullOrWhiteSpace(document.ExtractedText) || document.ExtractedText.StartsWith("OCR failed"))
                return ApiResponse.FaliureResponse("Document text hasn't been extracted yet or extraction failed. Please wait a moment or try re-uploading.");

            try
            {
                // Resolve AI service through scope to ensure thread safety or DI lifestyle
                using var scope = _scopeFactory.CreateScope();
                var summaryService = scope.ServiceProvider.GetRequiredService<ILlmSummaryService>();

                var summary = await summaryService.SummarizeMedicalTextAsync(document.ExtractedText);
                
                document.PatientSummary = summary;
                // Keep Processed status since it was already set after OCR, but update is fine
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse.SuccessResponse("Medical summary generated successfully.", new { summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary for document {DocId}", documentId);
                return ApiResponse.FaliureResponse("Error generating medical summary. Please try again later.");
            }
        }

        public async Task<ApiResponse> GetHistorySummaryAsync(string userId)
        {
            try
            {
                // Fetch all documents for this patient that have a medical summary
                var documents = await _unitOfWork.MedicalDocuments.FindAsync(d => 
                    d.PatientId == userId && 
                    !string.IsNullOrEmpty(d.PatientSummary) && 
                    !d.PatientSummary.Contains("not medical"));

                if (documents == null || !documents.Any())
                {
                    return ApiResponse.SuccessResponse("No completed medical summaries found in your history to aggregate.", new { summary = "No medical history available yet." });
                }

                // Format summaries chronologically for the AI
                var chronologicallyOrderedSummaries = documents
                    .OrderBy(d => d.UploadedAt)
                    .Select(d => $"[Date: {d.UploadedAt:yyyy-MM-dd}] [File: {d.FileName}]\nSUMMARY: {d.PatientSummary}")
                    .ToList();

                using var scope = _scopeFactory.CreateScope();
                var summaryService = scope.ServiceProvider.GetRequiredService<ILlmSummaryService>();

                var masterSummary = await summaryService.SummarizeMedicalHistoryAsync(chronologicallyOrderedSummaries);

                return ApiResponse.SuccessResponse("Master health overview generated successfully.", new { summary = masterSummary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating history summary for user {UserId}", userId);
                return ApiResponse.FaliureResponse("An error occurred while generating your health overview. Please try again later.");
            }
        }

        public async Task<ApiResponse> GetDocumentSummaryAsync(int documentId, string userId)
        {
            var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(documentId);
            if (document == null)
                return ApiResponse.FaliureResponse("Document not found");

            if (document.PatientId != userId)
                return ApiResponse.FaliureResponse("Unauthorized to access this document summary");

            if (string.IsNullOrWhiteSpace(document.PatientSummary))
            {
                return ApiResponse.FaliureResponse("This document has not been summarized yet. Please trigger the summarization first.");
            }

            return ApiResponse.SuccessResponse("Document summary retrieved successfully.", new { summary = document.PatientSummary });
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
