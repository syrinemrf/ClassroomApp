using System.ComponentModel.DataAnnotations;

namespace ClassroomApp.Models.ViewModels.Admin
{
    public class CreateTeacherViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }
    }
}
