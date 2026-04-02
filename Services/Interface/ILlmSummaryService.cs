namespace Sehha360.Services.Interface
{
    public interface ILlmSummaryService
    {
        Task<string> SummarizeMedicalTextAsync(string extractedText);
        Task<string> SummarizeMedicalHistoryAsync(IEnumerable<string> summaryHistory);
    }
}
