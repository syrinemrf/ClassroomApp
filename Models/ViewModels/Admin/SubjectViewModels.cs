using System.ComponentModel.DataAnnotations;

namespace ClassroomApp.Models.ViewModels.Admin
{
    public class CreateSubjectViewModel
    {
        [Required(ErrorMessage = "Le nom de la matière est requis")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Veuillez choisir une couleur")]
        public string Color { get; set; } = "#4F46E5";

        [Required(ErrorMessage = "L'enseignant est requis")]
        public Guid TeacherId { get; set; }

        [Required(ErrorMessage = "Sélectionnez au moins une classe")]
        public List<Guid> ClassroomIds { get; set; } = new();

        // Pour la vue
        public Dictionary<Guid, string>? AvailableTeachers { get; set; }
        public Dictionary<Guid, string>? AvailableClassrooms { get; set; }
    }

    public class EditSubjectViewModel : CreateSubjectViewModel
    {
        public Guid Id { get; set; }
    }

    public class SubjectListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#4F46E5";
        public string TeacherName { get; set; } = string.Empty;
        public List<string> ClassroomNames { get; set; } = new();
        public DateTime CreatedAt { get; set; }

        /// <summary>Couleur avec opacité 15% pour le fond des badges (hex+15 approximé)</summary>
        public string Color20 => Color + "26";
    }
}
