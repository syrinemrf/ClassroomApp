namespace ClassroomApp.Helpers
{
    public static class FileHelper
    {
        public static string GetFileTypeIcon(string contentType)
        {
            return contentType switch
            {
                "application/pdf" => "fa-file-pdf text-danger",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "fa-file-word text-primary",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" => "fa-file-powerpoint text-warning",
                "application/zip" => "fa-file-zipper text-secondary",
                var ct when ct.StartsWith("image/") => "fa-file-image text-success",
                _ => "fa-file text-muted"
            };
        }

        public static string FormatFileSize(long bytes)
        {
            return bytes switch
            {
                < 1024 => $"{bytes} B",
                < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
                _ => $"{bytes / (1024.0 * 1024.0):F1} MB"
            };
        }

        public static string GetInitials(string firstName, string lastName)
        {
            var f = string.IsNullOrEmpty(firstName) ? "" : firstName[..1].ToUpper();
            var l = string.IsNullOrEmpty(lastName) ? "" : lastName[..1].ToUpper();
            return f + l;
        }
    }
}
