using ClassroomApp.Models.ViewModels.Assignment;
using ClassroomApp.Models.ViewModels.Course;

namespace ClassroomApp.Models.ViewModels.Subject
{
    public class SubjectCardViewModel
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#1565C0";
        public string TeacherName { get; set; } = string.Empty;
        public string? TeacherProfilePicture { get; set; }
        public string TeacherInitials { get; set; } = string.Empty;
        public List<string> ClassroomNames { get; set; } = new();
        public int CourseCount { get; set; }
        public int AssignmentCount { get; set; }
        public int PendingAssignments { get; set; }
        public int StudentCount { get; set; }
    }

    public class SubjectDetailsViewModel
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectDescription { get; set; }
        public string Color { get; set; } = "#1565C0";
        public string TeacherName { get; set; } = string.Empty;
        public string? TeacherProfilePicture { get; set; }
        public string TeacherInitials { get; set; } = string.Empty;
        public Guid TeacherUserId { get; set; }
        public bool IsTeacher { get; set; }

        public List<ClassroomInfo> Classrooms { get; set; } = new();
        public Guid? SelectedClassroomId { get; set; }

        // Stream: recent activity (combined courses + assignments)
        public List<StreamItem> StreamItems { get; set; } = new();

        // Classwork
        public List<CourseViewModel> Courses { get; set; } = new();
        public List<AssignmentViewModel> Assignments { get; set; } = new();

        // People
        public List<PersonInfo> Students { get; set; } = new();
    }

    public class ClassroomInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }

    public class StreamItem
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Course" or "Assignment"
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorPicture { get; set; }
        public string AuthorInitials { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public string? FileName { get; set; }
        public string? FileIcon { get; set; }
    }

    public class PersonInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string Initials { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
    }
}