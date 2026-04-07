using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Sehha360.Services.Interface;

namespace Sehha360.Services.implementation
{
    public class GroqSummaryService : ILlmSummaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GroqSummaryService> _logger;
        private readonly string _apiKey;
        private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

        public GroqSummaryService(ILogger<GroqSummaryService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            
            // Allow override via environment variable, strip quotes and spaces
            _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")?.Trim()?.Trim('"')?.Trim('\'') ?? string.Empty;
        }

        public async Task<string> SummarizeMedicalTextAsync(string extractedText)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
                return "No text available to summarize.";

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Groq API Key is missing. Medical summarization will be skipped.");
                return "Summarization skipped: Groq API key is not configured. Please add GROQ_API_KEY to your environment variables.";
            }

            try
            {
                var prompt = @"You are a strict medical parser for the Sehha360 app.
Analyze the following extracted text from a user upload.

STRICT INSTRUCTIONS:
1. First, process the information internally in English to ensure accuracy, but YOUR FINAL AND ONLY OUTPUT MUST BE EXCLUSIVELY IN SIMPLE EGYPTIAN ARABIC. DO NOT output any English text.
2. If the text is NOT a medical document, health-related test result, or prescription, you MUST reply with EXACTLY this phrase and NOTHING ELSE: ""Provided document is not medical. Please upload a medical document.""
3. If it IS a medical document, provide a very simple, plain-language summary of the findings that anyone without a medical background can easily understand. Address the user directly in the second person (e.g., ""نتائج فحوصاتك بتوضح..."").
4. Avoid complicated medical terms, jargon, and complex descriptions. If a medical term must be used, explain it in the simplest possible everyday Egyptian Arabic words.
5. DO NOT use any Markdown formatting (no asterisks, bolding, italics, or hash symbols). Return ONLY raw plain text.
6. DO NOT include ANY greetings, conversational filler, or introductions. Start directly with the summary data.
7. ALWAYS end with a disclaimer: ""هذا ملخص تم إنشاؤه بواسطة الذكاء الاصطناعي. يرجى استشارة طبيبك.""
8. Keep the output clean, strict, and purely informative.

TEXT TO ANALYZE:
" + extractedText;

                var payload = new
                {
                    model = "llama-3.3-70b-versatile",
                    temperature = 1,
                    // max_completion_tokens = 8192,
                    top_p = 1,
                    stream = false,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                using var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, GroqUrl);
                requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");
                requestMessage.Content = requestContent;

                var response = await _httpClient.SendAsync(requestMessage);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Groq API failed: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return $"Summarization failed: HTTP {response.StatusCode} from Groq API. Please try again later.";
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
                    {
                        var rawSummary = content.GetString()?.Trim() ?? "Failed to generate a readable summary.";
                        return SanitizeArabicText(rawSummary);
                    }
                }

                return "Unexpected API response format from Groq.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Groq medical summarization.");
                return $"Summarization error: {ex.Message}";
            }
        }
        public async Task<string> SummarizeMedicalHistoryAsync(IEnumerable<string> summaryHistory)
        {
            if (summaryHistory == null || !summaryHistory.Any())
                return "No medical history available to summarize.";

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Groq API Key is missing. History summarization will be skipped.");
                return "Summarization skipped: Groq API key is not configured.";
            }

            try
            {
                var historyText = string.Join("\n\n---\n\n", summaryHistory);
                var prompt = @"You are a medical history aggregator for the Sehha360 app. 
You will be provided with a series of individual medical reports and test summaries belonging to a single patient, ordered chronologically.
Your task is to analyze these summaries and provide a single, high-level ""Master Health Overview"".

STRICT RULES:
1. First, process the information internally in English to ensure accuracy, but YOUR FINAL AND ONLY OUTPUT MUST BE EXCLUSIVELY IN SIMPLE EGYPTIAN ARABIC. DO NOT output any English text.
2. If the summary history is not medical or too poor to aggregate, return ONLY: ""No medical history available to summarize.""
3. Summarize the patient's overall health journey and identify trends using extremely simple, everyday Egyptian Arabic. Address the user directly in the second person (e.g., ""تاريخك الطبي بيوضح..."").
4. DO NOT use complicated medical terms, jargon, or complex descriptions. Explain any necessary medical concepts so that a layperson can easily understand them.
5. DO NOT use any Markdown formatting (no asterisks, bolding, italics, or hash symbols). Return ONLY raw plain text.
6. DO NOT include any greetings, introductions, or conversational filler.
7. ALWAYS end with a disclaimer: ""هذا ملخص تم إنشاؤه بواسطة الذكاء الاصطناعي. يرجى استشارة طبيبك.""

PATIENT SUMMARY HISTORY:
" + historyText;

                var payload = new
                {
                    model = "llama-3.3-70b-versatile",
                    temperature = 0.7,
                    // max_completion_tokens = 8192,
                    top_p = 1,
                    stream = false,
                    messages = new []
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                using var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, GroqUrl);
                requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");
                requestMessage.Content = requestContent;

                var response = await _httpClient.SendAsync(requestMessage);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Groq API failed: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return $"History summarization failed: HTTP {response.StatusCode} from Groq API.";
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
                    {
                        var rawSummary = content.GetString()?.Trim() ?? "Failed to generate a readable history overview.";
                        return SanitizeArabicText(rawSummary);
                    }
                }

                return "Unexpected API response format from Groq during history aggregation.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Groq history summarization.");
                return $"Summarization error: {ex.Message}";
            }
        }
        private string SanitizeArabicText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            return Regex.Replace(input, @"[^\p{IsArabic}\s\d.,!؟""'،؛\-%()]+", "");
        }
    }
}
