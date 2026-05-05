using System.ComponentModel.DataAnnotations;

namespace ClassroomApp.Models.ViewModels.Admin
{
    public class CreateClassroomViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "L'année académique est requise")]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;
    }
}
