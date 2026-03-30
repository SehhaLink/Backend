using System.Text.Json;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class OcrSpaceService : IOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OcrSpaceService> _logger;
        private readonly string _apiKey;

        public OcrSpaceService(ILogger<OcrSpaceService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            
            _apiKey = Environment.GetEnvironmentVariable("OCR_SPACE_API_KEY") ?? "helloworld";
        }

        public async Task<string> ExtractTextAsync(Stream fileStream, string mimeType)
        {
            try
            {
                using var request = new MultipartFormDataContent();
                
                request.Add(new StringContent(_apiKey), "apikey");
                request.Add(new StringContent("eng"), "language");
                request.Add(new StringContent("true"), "scale"); 
                request.Add(new StringContent("true"), "isTable"); 
                request.Add(new StringContent("2"), "OCREngine"); 

                var fileContent = new StreamContent(fileStream);
                
                string fakeFileName = mimeType.Contains("pdf") ? "document.pdf" : "document.png";
                request.Add(fileContent, "file", fakeFileName);

                var response = await _httpClient.PostAsync("https://api.ocr.space/parse/image", request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OCR.Space API failed: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return $"OCR failed: HTTP {response.StatusCode}";
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("IsErroredOnProcessing", out var isErrored) && isErrored.GetBoolean())
                {
                    var errorMessage = root.GetProperty("ErrorMessage").EnumerateArray().FirstOrDefault().GetString();
                    _logger.LogError("OCR.Space Processing Error: {Error}", errorMessage);
                    return $"OCR Processing Error: {errorMessage}";
                }

                if (root.TryGetProperty("ParsedResults", out var parsedArray) && parsedArray.GetArrayLength() > 0)
                {
                    List<string> pagesText = new List<string>();
                    foreach (var parsedPage in parsedArray.EnumerateArray())
                    {
                        if (parsedPage.TryGetProperty("ParsedText", out var textProp))
                        {
                            pagesText.Add(textProp.GetString() ?? "");
                        }
                    }
                    
                    return string.Join("\n\n---\n\n", pagesText);
                }

                return "No text could be extracted from the document.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during OCR.Space processing.");
                return $"OCR exception: {ex.Message}";
            }
        }
    }
}
