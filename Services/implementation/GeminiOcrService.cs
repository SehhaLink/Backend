using System.Text;
using System.Text.Json;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class GeminiOcrService : IOcrService
    {
        private readonly string _apiKey;
        private readonly ILogger<GeminiOcrService> _logger;
        private readonly HttpClient _httpClient;

        private const string GeminiApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public GeminiOcrService(IConfiguration configuration, ILogger<GeminiOcrService> logger)
        {
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? configuration["Gemini:ApiKey"]
                      ?? string.Empty;
            _logger = logger;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
        }

        public async Task<string> ExtractTextAsync(Stream fileStream, string mimeType)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Gemini API Key is missing. OCR will be skipped.");
                return "OCR skipped: API Key missing.";
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Data = Convert.ToBase64String(fileBytes);

                var prompt = "Extract all text from this medical document and return it as formatted Markdown. " +
                             "Ensure medical terms are correctly transcribed. " +
                             "If it is a native PDF, extract text directly. If it is a scan/image, perform high-quality OCR. " +
                             "Maintain the structure of the document (headings, lists, tables) where possible.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { inline_data = new { mime_type = mimeType, data = base64Data } },
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{GeminiApiBaseUrl}?key={_apiKey}";
                HttpResponseMessage? response = null;
                string responseBody = string.Empty;

                int maxRetries = 4;
                for (int i = 0; i < maxRetries; i++)
                {
                    response = await _httpClient.PostAsync(url, content);
                    responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, i + 1));
                        _logger.LogWarning("Gemini OCR Rate Limited (429). Retrying in {Delay} seconds...", delay.TotalSeconds);
                        await Task.Delay(delay);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                        continue;
                    }

                    break; 
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    var statusCode = response?.StatusCode.ToString() ?? "Unknown";
                    _logger.LogError("Gemini API returned {StatusCode}: {Body}", statusCode, responseBody);
                    return $"OCR failed: API returned {statusCode}";
                }

                // Parse the response to extract the text
                using var doc = JsonDocument.Parse(responseBody);
                var candidates = doc.RootElement.GetProperty("candidates");
                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Gemini OCR processing");
                return $"OCR failed: {ex.Message}";
            }
        }
    }
}