namespace ClassroomApp.Services
{
    public interface IAIService
    {
        Task<string> AskQuestionAsync(string question, string? context = null);
    }
}
