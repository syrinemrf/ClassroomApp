using System.Text;
using System.Text.Json;

namespace ClassroomApp.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiService> _logger;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public GeminiService(IConfiguration config, ILogger<GeminiService> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> GetAIResponseAsync(string userMessage, string? context = null)
        {
            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("YOUR_GEMINI"))
            {
                _logger.LogWarning("Gemini API key not configured.");
                return "L'assistant IA n'est pas configuré. Veuillez définir la clé Gemini:ApiKey.";
            }

            try
            {
                var systemPrompt = context ?? @"Tu es un assistant IA pour une plateforme de gestion des cours universitaires appelée ClassroomApp.
Tu aides les enseignants et les étudiants avec des questions académiques, des conseils d'étude, des clarifications sur les devoirs et les cours.
Réponds toujours en français. Sois professionnel, courtois et utile. Fournis des explications claires et pertinentes au contexte académique.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = systemPrompt },
                                new { text = userMessage }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 1024
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"{BaseUrl}?key={apiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini API error: {response.StatusCode} - {errorBody}");
                    return "Une erreur s'est produite lors de la communication avec l'IA.";
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseBody);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentProp))
                    {
                        if (contentProp.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            if (parts[0].TryGetProperty("text", out var textProp))
                            {
                                return textProp.GetString() ?? "Pas de réponse générée.";
                            }
                        }
                    }
                }

                return "Impossible de traiter la réponse de l'IA.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return "Une erreur s'est produite. Veuillez réessayer plus tard.";
            }
        }
    }
}
