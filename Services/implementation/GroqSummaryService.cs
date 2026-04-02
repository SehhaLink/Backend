using System.Text;
using System.Text.Json;
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
                var prompt = @"You are a strict medical parser for the Sehha360 app. Analyze the following extracted text from a user upload.
First, determine if the text is a medical document, health-related test result, or prescription.
If it is NOT a medical document, you MUST reply with EXACTLY this phrase and nothing else: ""Provided document is not medical. Please upload a medical document.""
If it IS a medical document, provide a summary explaining the findings in simple, patient-friendly terms. Clarify any medical jargon or numbers.
CRITICAL RULES:
1. DO NOT include ANY greetings, conversational filler, introductions, or closing encouragements (e.g., no ""Hello"", no ""Here is your summary""). Start directly with the markdown formatted data.
2. Tell the patient to always consult their doctor at the very end.
3. Keep the output clean and strict.

TEXT TO ANALYZE:
" + extractedText;

                var payload = new
                {
                    model = "openai/gpt-oss-120b",
                    temperature = 1,
                    max_completion_tokens = 8192,
                    top_p = 1,
                    stream = false, // Must be false for background DB JSON parsing
                    reasoning_effort = "medium",
                    stop = (string?)null,
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
                        return content.GetString()?.Trim() ?? "Failed to generate a readable summary.";
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

GOALS:
1. Summarize the patient's overall health journey.
2. Identify trends (e.g. ""Your blood pressure has consistently decreased over the last 6 months"").
3. Note any recurring issues or significant improvements.
4. Provide a concise, structured timeline of major events.

STRICT RULES:
1. DO NOT include any greetings, introductions, or conversational filler.
2. Use professional yet simple, patient-friendly language.
3. Use Markdown for formatting (headings, bullet points).
4. ALWAYS end with a disclaimer: ""This is an AI-generated aggregation. Please consult your physician for a full clinical review of your history.""

PATIENT SUMMARY HISTORY:
" + historyText;

                var payload = new
                {
                    model = "openai/gpt-oss-120b",
                    temperature = 0.7, // Slightly lower for history aggregation to be more grounded
                    max_completion_tokens = 8192,
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
                        return content.GetString()?.Trim() ?? "Failed to generate a readable history overview.";
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
    }
}
