using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClassroomApp.Models.ViewModels.Course
{
    public class CreateCourseViewModel
    {
        [Required(ErrorMessage = "Le titre est requis")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Veuillez s\u00e9lectionner une mati\u00e8re")]
        public Guid SubjectId { get; set; }

        [Required(ErrorMessage = "Veuillez s\u00e9lectionner au moins une classe")]
        public List<Guid> ClassroomIds { get; set; } = new();

        [Required(ErrorMessage = "Veuillez s\u00e9lectionner un fichier")]
        public IFormFile File { get; set; } = null!;

        public Dictionary<Guid, string>? AvailableSubjects { get; set; }
        public Dictionary<Guid, string>? AvailableClassrooms { get; set; }
    }
}
