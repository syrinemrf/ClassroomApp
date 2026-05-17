using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Admin", "Teacher", "Student")]
    public class ChatbotController : Controller
    {
        private readonly IGeminiService _geminiService;
        private readonly AppDbContext _context;

        public ChatbotController(IGeminiService geminiService, AppDbContext context)
        {
            _geminiService = geminiService;
            _context = context;
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
                // Build context for students
                string? context = null;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "Student")
                {
                    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                    var student = await _context.Students
                        .Include(s => s.Classroom)
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (student != null)
                    {
                        var courseNames = await _context.CourseClassrooms
                            .Where(cc => cc.ClassroomId == student.ClassroomId)
                            .Select(cc => cc.Course.Title)
                            .ToListAsync();

                        var subjects = await _context.SubjectClassrooms
                            .Where(sc => sc.ClassroomId == student.ClassroomId)
                            .Select(sc => sc.Subject.Name)
                            .ToListAsync();

                        context = $@"Tu es un assistant pédagogique IA pour la plateforme ClassroomApp.
Tu aides exclusivement les étudiants dans leur apprentissage académique : comprendre les cours, faire les devoirs, réviser, et développer des méthodes de travail efficaces.
Réponds toujours en français. Sois pédagogique, bienveillant, clair et encourageant.

Contexte de l'étudiant :
- Classe : {student.Classroom.Name}
{(subjects.Any() ? $"- Matières : {string.Join(", ", subjects)}" : "")}
{(courseNames.Any() ? $"- Cours disponibles : {string.Join(", ", courseNames)}" : "")}

Si l'étudiant pose une question hors-sujet académique, redirige-le poliment vers ses études.";
                    }
                }

                var response = await _geminiService.GetAIResponseAsync(request.Question, context);
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
