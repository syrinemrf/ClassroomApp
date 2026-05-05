namespace ClassroomApp.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        void DeleteFile(string relativePath);
        FileStream? GetFileStream(string relativePath);
        bool IsAllowedCourseFile(IFormFile file);
        bool IsAllowedSubmissionFile(IFormFile file);
        bool IsAllowedProfileImage(IFormFile file);
    }
}
