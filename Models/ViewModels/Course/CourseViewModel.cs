namespace ClassroomApp.Models.ViewModels.Course
{
    public class CourseViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public List<string> ClassroomNames { get; set; } = new();
        public List<Guid> ClassroomIds { get; set; } = new();
        public Guid TeacherId { get; set; }

        public string FileSizeFormatted =>
            FileSize switch
            {
                < 1024 => $"{FileSize} B",
                < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
                _ => $"{FileSize / (1024.0 * 1024.0):F1} MB"
            };

        public string FileTypeIcon =>
            ContentType switch
            {
                "application/pdf" => "fa-file-pdf text-danger",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "fa-file-word text-primary",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" => "fa-file-powerpoint text-warning",
                "application/zip" => "fa-file-zipper text-secondary",
                var ct when ct.StartsWith("image/") => "fa-file-image text-success",
                _ => "fa-file text-muted"
            };
    }

    public class EditCourseViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string FileName { get; set; } = string.Empty;
        public List<string> ClassroomNames { get; set; } = new();
    }
}
