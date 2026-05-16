using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClassroomApp.Models.ViewModels.Assignment
{
    public class CreateAssignmentViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(5000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Deadline is required")]
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required(ErrorMessage = "Max score is required")]
        [Range(1, 1000)]
        public int MaxScore { get; set; } = 100;

        [Required(ErrorMessage = "Classroom is required")]
        public Guid ClassroomId { get; set; }

        public Guid? SubjectId { get; set; }

        /// <summary>Optional file attachment (PDF, DOCX, PPTX, ZIP, images – max 50 MB).</summary>
        public IFormFile? AttachmentFile { get; set; }

        /// <summary>Displayed when editing an assignment that already has a file.</summary>
        public string? ExistingFileName { get; set; }

        public Dictionary<Guid, string>? AvailableClassrooms { get; set; }
        public Dictionary<Guid, string>? AvailableSubjects { get; set; }
    }
}
