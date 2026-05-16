using ClassroomApp.Filters;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Admin", "Teacher", "Student")]
    public class ChatbotController : Controller
    {
        private readonly IGeminiService _geminiService;

        public ChatbotController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AskQuestion([FromBody] ChatbotRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
            {
                return Json(new { error = "La question ne peut pas être vide." });
            }

            try
            {
                var response = await _geminiService.GetAIResponseAsync(request.Question);
                return Json(new { response });
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Erreur : {ex.Message}" });
            }
        }
    }

    public class ChatbotRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}
