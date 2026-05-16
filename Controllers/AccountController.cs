using ClassroomApp.Data;
using ClassroomApp.Models.ViewModels.Auth;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IFileService _fileService;
        private readonly AppDbContext _context;

        public AccountController(IAuthService authService, IFileService fileService, AppDbContext context)
        {
            _authService = authService;
            _fileService = fileService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToDashboard();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var principal = await _authService.ValidateLoginAsync(model.Email, model.Password);
            if (principal == null)
            {
                TempData["Error"] = "Invalid email or password.";
                return View(model);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users
                .Include(u => u.Teacher).ThenInclude(t => t!.Courses)
                .Include(u => u.Teacher).ThenInclude(t => t!.Assignments)
                .Include(u => u.Student).ThenInclude(s => s!.Classroom)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var vm = new ProfileViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                ProfilePicturePath = user.ProfilePicturePath,
                CreatedAt = user.CreatedAt,
                Department = user.Teacher?.Department,
                Bio = user.Teacher?.Bio,
                StudentNumber = user.Student?.StudentNumber,
                ClassroomName = user.Student?.Classroom?.Name,
                TotalCourses = user.Teacher?.Courses?.Count ?? 0,
                TotalAssignments = user.Teacher?.Assignments?.Count ?? 0,
                TotalClassrooms = user.Teacher != null
                    ? await _context.CourseClassrooms.Where(cc => cc.Course.TeacherId == user.Teacher.Id).Select(cc => cc.ClassroomId).Distinct().CountAsync()
                    : 0
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _authService.UpdateProfileAsync(userId, model.FirstName, model.LastName, model.Department, model.Bio);

            if (result)
                TempData["Success"] = "Profile updated successfully.";
            else
                TempData["Error"] = "Failed to update profile.";

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(Profile));
            }

            if (!_fileService.IsAllowedProfileImage(profilePicture))
            {
                TempData["Error"] = "Only JPG, PNG, WEBP images under 2MB are allowed.";
                return RedirectToAction(nameof(Profile));
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Delete the old profile picture file if it exists
            var user = await _context.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.ProfilePicturePath))
                _fileService.DeleteFile(user.ProfilePicturePath);

            var path = await _fileService.SaveFileAsync(profilePicture, "profiles");
            await _authService.UploadProfilePictureAsync(userId, path);

            // Refresh the auth cookie so the ProfilePicture claim reflects the new picture
            // (layouts read the claim to render the avatar in the navbar)
            var claims = User.Claims
                .Where(c => c.Type != "ProfilePicture")
                .Append(new System.Security.Claims.Claim("ProfilePicture", path))
                .ToList();

            var identity = new System.Security.Claims.ClaimsIdentity(
                claims,
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                new System.Security.Claims.ClaimsPrincipal(identity),
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            TempData["Success"] = "Profile picture updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToDashboard()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                "Student" => RedirectToAction("Dashboard", "Student"),
                _ => RedirectToAction(nameof(Login))
            };
        }
    }
}
