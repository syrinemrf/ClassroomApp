using System.ComponentModel.DataAnnotations;
using ClassroomApp.Models.Enums;

namespace ClassroomApp.Models.ViewModels.Auth
{
    public class ProfileViewModel
    {
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public Role Role { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string? Department { get; set; }
        public string? Bio { get; set; }
        public string? StudentNumber { get; set; }
        public string? ClassroomName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalClassrooms { get; set; }
    }
}
