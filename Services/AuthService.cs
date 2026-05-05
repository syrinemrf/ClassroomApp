using System.Security.Claims;
using ClassroomApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ClassroomApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClaimsPrincipal?> ValidateLoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("ProfilePicture", user.ProfilePicturePath ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, string firstName, string lastName, string? department, string? bio)
        {
            var user = await _context.Users
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.FirstName = firstName;
            user.LastName = lastName;

            if (user.Teacher != null)
            {
                user.Teacher.Department = department;
                user.Teacher.Bio = bio;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadProfilePictureAsync(Guid userId, string filePath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.ProfilePicturePath = filePath;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
