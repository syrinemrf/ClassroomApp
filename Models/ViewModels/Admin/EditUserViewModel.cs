using ClassroomApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ClassroomApp.Models.ViewModels.Admin
{
    public class EditUserViewModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public Role Role { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        // For students
        public Guid? ClassroomId { get; set; }
        public Dictionary<Guid, string>? AvailableClassrooms { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string? ConfirmPassword { get; set; }

        public bool IsActive { get; set; }
    }
}
