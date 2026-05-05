using OpenAI.Chat;

namespace ClassroomApp.Services
{
    public class AIService : IAIService
    {
        private readonly IConfiguration _configuration;

        public AIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> AskQuestionAsync(string question, string? context = null)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"] ?? "gpt-4o";

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY")
                return "AI service is not configured. Please set the OpenAI API key in appsettings.json.";

            try
            {
                var client = new ChatClient(model, apiKey);

                var systemMessage = "You are an educational assistant helping students understand their coursework. " +
                    "Be clear, pedagogical, and encouraging. Give concise but helpful answers.";

                if (!string.IsNullOrEmpty(context))
                    systemMessage += $"\n\nStudent context: {context}";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemMessage),
                    new UserChatMessage(question)
                };

                ChatCompletion completion = await client.CompleteChatAsync(messages);
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                return $"Sorry, I encountered an error: {ex.Message}";
            }
        }
    }
}
