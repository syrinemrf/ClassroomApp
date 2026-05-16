namespace ClassroomApp.Services
{
    public interface IGeminiService
    {
        Task<string> GetAIResponseAsync(string userMessage, string? context = null);
    }
}
