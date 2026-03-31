using System.Text;
using System.Text.Json;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class PaddleOcrApiService : IOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaddleOcrApiService> _logger;
        private readonly string _apiToken;
        private readonly string _apiUrl;

        public PaddleOcrApiService(ILogger<PaddleOcrApiService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            
            _apiToken = Environment.GetEnvironmentVariable("PADDLE_OCR_TOKEN");
            _apiUrl = Environment.GetEnvironmentVariable("PADDLE_OCR_URL");
        }

        public async Task<string> ExtractTextAsync(Stream fileStream, string mimeType)
        {
            if (string.IsNullOrEmpty(_apiToken))
            {
                _logger.LogError("PaddleOCR Token is missing in environment variables.");
                return "OCR Configuration Error: Token is missing.";
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var fileData = Convert.ToBase64String(fileBytes);

                int fileType = mimeType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

                var payload = new
                {
                    file = fileData,
                    fileType = fileType,
                    useDocOrientationClassify = false,
                    useDocUnwarping = false,
                    useChartRecognition = false
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                using var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
                requestMessage.Headers.Add("Authorization", $"token {_apiToken}");
                requestMessage.Content = requestContent;

                var response = await _httpClient.SendAsync(requestMessage);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PaddleOCR API failed: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return $"OCR failed: HTTP {response.StatusCode} - {responseBody}";
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("result", out var resultNode) && 
                    resultNode.TryGetProperty("layoutParsingResults", out var resultsArray) && 
                    resultsArray.GetArrayLength() > 0)
                {
                    var extractedTexts = new List<string>();

                    foreach (var res in resultsArray.EnumerateArray())
                    {
                        if (res.TryGetProperty("markdown", out var markdownNode) && 
                            markdownNode.TryGetProperty("text", out var textNode))
                        {
                            var text = textNode.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                extractedTexts.Add(text);
                            }
                        }
                    }

                    if (extractedTexts.Count > 0)
                    {
                        return string.Join("\n\n---\n\n", extractedTexts);
                    }
                }

                return "No text could be extracted from the document using PaddleOCR.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during PaddleOCR API processing.");
                return $"OCR exception: {ex.Message}";
            }
        }
    }
}
