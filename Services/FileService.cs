namespace ClassroomApp.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadsRoot;

        private static readonly string[] AllowedCourseExtensions = { ".pdf", ".docx", ".pptx", ".zip", ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedSubmissionExtensions = { ".pdf", ".docx", ".pptx", ".zip", ".jpg", ".jpeg", ".png", ".webp", ".txt" };
        private static readonly string[] AllowedProfileExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxCourseSize = 50 * 1024 * 1024; // 50 MB
        private const long MaxSubmissionSize = 20 * 1024 * 1024; // 20 MB
        private const long MaxProfileSize = 2 * 1024 * 1024; // 2 MB

        public FileService(IConfiguration configuration)
        {
            var configuredUploadsRoot = configuration["Storage:UploadsRoot"];
            _uploadsRoot = string.IsNullOrWhiteSpace(configuredUploadsRoot)
                ? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ClassroomApp",
                    "uploads")
                : configuredUploadsRoot;

            if (string.IsNullOrWhiteSpace(_uploadsRoot))
            {
                _uploadsRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ClassroomApp",
                    "uploads");
            }

            Directory.CreateDirectory(_uploadsRoot);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsPath = Path.Combine(_uploadsRoot, folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            var fullPath = ResolveFullPath(relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public FileStream? GetFileStream(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            var fullPath = ResolveFullPath(relativePath);
            return File.Exists(fullPath) ? new FileStream(fullPath, FileMode.Open, FileAccess.Read) : null;
        }

        private string ResolveFullPath(string relativePath)
        {
            var normalizedPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            const string uploadsPrefix = "uploads";

            if (normalizedPath.StartsWith(uploadsPrefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath[(uploadsPrefix.Length + 1)..];
            }

            return Path.Combine(_uploadsRoot, normalizedPath);
        }

        public bool IsAllowedCourseFile(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedCourseExtensions.Contains(ext) && file.Length <= MaxCourseSize;
        }

        public bool IsAllowedSubmissionFile(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedSubmissionExtensions.Contains(ext) && file.Length <= MaxSubmissionSize;
        }

        public bool IsAllowedProfileImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedProfileExtensions.Contains(ext) && file.Length <= MaxProfileSize;
        }
    }
}
