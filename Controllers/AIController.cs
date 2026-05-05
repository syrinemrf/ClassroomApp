using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Student")]
    public class AIController : Controller
    {
        private readonly IAIService _aiService;
        private readonly AppDbContext _context;

        public AIController(IAIService aiService, AppDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return Json(new { error = "Please enter a question." });

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students
                .Include(s => s.Classroom)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            var context = "";
            if (student != null)
            {
                var courseNames = await _context.CourseClassrooms
                    .Where(cc => cc.ClassroomId == student.ClassroomId)
                    .Select(cc => cc.Course.Title)
                    .ToListAsync();
                context = $"Classe : {student.Classroom.Name}. ";
                if (courseNames.Any())
                    context += $"Cours : {string.Join(", ", courseNames)}.";
            }

            var response = await _aiService.AskQuestionAsync(request.Question, context);
            return Json(new { response });
        }

        public class AskRequest
        {
            public string Question { get; set; } = string.Empty;
        }
    }
}
