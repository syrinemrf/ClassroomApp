using System.Security.Claims;

namespace ClassroomApp.Services
{
    public interface IAuthService
    {
        Task<ClaimsPrincipal?> ValidateLoginAsync(string email, string password);
        Task<bool> UpdateProfileAsync(Guid userId, string firstName, string lastName, string? department, string? bio);
        Task<bool> UploadProfilePictureAsync(Guid userId, string filePath);
    }
}
