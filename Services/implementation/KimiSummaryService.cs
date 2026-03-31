using System.Text;
using System.Text.Json;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class KimiSummaryService : ILlmSummaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KimiSummaryService> _logger;
        private readonly string _apiKey;
        private const string KimiUrl = "https://api.moonshot.ai/v1/chat/completions";

        public KimiSummaryService(ILogger<KimiSummaryService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            
            _apiKey = Environment.GetEnvironmentVariable("KIMI_API_KEY") ?? string.Empty;
        }

        public async Task<string> SummarizeMedicalTextAsync(string extractedText)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
                return "No text available to summarize.";

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Kimi API Key is missing. Medical summarization will be skipped.");
                return "Summarization skipped: Kimi API key is not configured. Please add KIMI_API_KEY to your environment variables.";
            }

            try
            {
                var prompt = "You are a helpful medical assistant for Sehha360 app. A patient has uploaded their medical document or test results. Explain the findings in simple, easily understandable words for the patient. Clarify what complex medical jargon or numbers mean for an average person. Be concise, empathetic, and strictly respond cleanly in markdown format. Do not provide a formal medical diagnosis. Tell the patient to always consult their doctor and if the text in the document is not medical text say that it is not medical text and ask the patient to upload a medical document. Here is the extracted text from their document:\n\n" + extractedText;

                var payload = new
                {
                    model = "moonshot-v1-8k",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                using var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, KimiUrl);
                requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");
                requestMessage.Content = requestContent;

                var response = await _httpClient.SendAsync(requestMessage);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Kimi API failed: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return $"Summarization failed: HTTP {response.StatusCode} from Kimi API. Please try again later.";
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
                    {
                        return content.GetString()?.Trim() ?? "Failed to generate a readable summary.";
                    }
                }

                return "Unexpected API response format from Kimi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Kimi medical summarization.");
                return $"Summarization error: {ex.Message}";
            }
        }
    }
}
