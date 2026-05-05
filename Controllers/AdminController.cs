using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using ClassroomApp.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ?????????????????????????????????????????????
        // TABLEAU DE BORD
        // ?????????????????????????????????????????????
        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalTeachers = await _context.Teachers.CountAsync(),
                TotalStudents = await _context.Students.CountAsync(),
                TotalClassrooms = await _context.Classrooms.CountAsync(),
                TotalCourses = await _context.Subjects.CountAsync(),
                TotalAssignments = await _context.Assignments.CountAsync()
            };

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentActivityItem
                {
                    Icon = u.Role == Role.Teacher ? "fa-chalkboard-teacher" : u.Role == Role.Student ? "fa-user-graduate" : "fa-user-shield",
                    IconColor = u.Role == Role.Teacher ? "text-primary" : u.Role == Role.Student ? "text-success" : "text-warning",
                    Description = $"{u.FirstName} {u.LastName} ({(u.Role == Role.Teacher ? "Enseignant" : u.Role == Role.Student ? "Étudiant" : "Admin")}) a été créé",
                    Timestamp = u.CreatedAt
                })
                .ToListAsync();
            vm.RecentActivity = recentUsers;

            return View(vm);
        }

        // ?????????????????????????????????????????????
        // ENSEIGNANTS
        // ?????????????????????????????????????????????
        public async Task<IActionResult> Teachers(int page = 1)
        {
            const int pageSize = 10;
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Subjects)
                .OrderBy(t => t.User.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await _context.Teachers.CountAsync() / (double)pageSize);
            return View(teachers);
        }

        [HttpGet]
        public IActionResult CreateTeacher() => View(new CreateTeacherViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(CreateTeacherViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(model);
            }

            var user = new User
            {
                Id = Guid.NewGuid(), FirstName = model.FirstName, LastName = model.LastName,
                Email = model.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = Role.Teacher, CreatedAt = DateTime.UtcNow, IsActive = true
            };
            _context.Users.Add(user);
            _context.Teachers.Add(new Teacher { Id = Guid.NewGuid(), UserId = user.Id, Department = model.Department, Bio = model.Bio });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Enseignant {model.FirstName} {model.LastName} créé avec succès.";
            return RedirectToAction(nameof(Teachers));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(Guid id)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) { TempData["Error"] = "Enseignant introuvable."; return RedirectToAction(nameof(Teachers)); }
            _context.Users.Remove(teacher.User);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Enseignant supprimé avec succès.";
            return RedirectToAction(nameof(Teachers));
        }

        // ?????????????????????????????????????????????
        // CLASSES (groupes d'étudiants)
        // ?????????????????????????????????????????????
        public async Task<IActionResult> Classrooms()
        {
            var classrooms = await _context.Classrooms
                .Include(c => c.Students)
                .Include(c => c.SubjectClassrooms).ThenInclude(sc => sc.Subject).ThenInclude(s => s.Teacher).ThenInclude(t => t.User)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(classrooms);
        }

        [HttpGet]
        public IActionResult CreateClassroom() => View(new CreateClassroomViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClassroom(CreateClassroomViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Classrooms.Add(new Classroom { Id = Guid.NewGuid(), Name = model.Name, Description = model.Description, AcademicYear = model.AcademicYear });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Classe '{model.Name}' créée avec succès.";
            return RedirectToAction(nameof(Classrooms));
        }

        [HttpGet]
        public async Task<IActionResult> EditClassroom(Guid id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null) return NotFound();
            var model = new CreateClassroomViewModel { Name = classroom.Name, Description = classroom.Description, AcademicYear = classroom.AcademicYear };
            ViewBag.ClassroomId = id;
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClassroom(Guid id, CreateClassroomViewModel model)
        {
            if (!ModelState.IsValid) { ViewBag.ClassroomId = id; return View(model); }
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null) return NotFound();
            classroom.Name = model.Name;
            classroom.Description = model.Description;
            classroom.AcademicYear = model.AcademicYear;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Classe modifiée avec succès.";
            return RedirectToAction(nameof(Classrooms));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClassroom(Guid id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null) { TempData["Error"] = "Classe introuvable."; return RedirectToAction(nameof(Classrooms)); }
            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Classe supprimée avec succès.";
            return RedirectToAction(nameof(Classrooms));
        }

        // ?????????????????????????????????????????????
        // MATIÈRES (Subjects)
        // ?????????????????????????????????????????????
        public async Task<IActionResult> Subjects()
        {
            var subjects = await _context.Subjects
                .Include(s => s.Teacher).ThenInclude(t => t.User)
                .Include(s => s.SubjectClassrooms).ThenInclude(sc => sc.Classroom)
                .OrderBy(s => s.Name)
                .Select(s => new SubjectListItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Color = s.Color,
                    TeacherName = s.Teacher.User.FirstName + " " + s.Teacher.User.LastName,
                    ClassroomNames = s.SubjectClassrooms.Select(sc => sc.Classroom.Name).ToList(),
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();
            return View(subjects);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSubject()
        {
            var model = new CreateSubjectViewModel
            {
                AvailableTeachers = await _context.Teachers.Include(t => t.User).OrderBy(t => t.User.LastName)
                    .ToDictionaryAsync(t => t.Id, t => $"{t.User.FirstName} {t.User.LastName}"),
                AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name)
                    .ToDictionaryAsync(c => c.Id, c => c.Name)
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubject(CreateSubjectViewModel model)
        {
            if (!ModelState.IsValid || !model.ClassroomIds.Any())
            {
                ModelState.AddModelError("ClassroomIds", "Sélectionnez au moins une classe.");
                model.AvailableTeachers = await _context.Teachers.Include(t => t.User).OrderBy(t => t.User.LastName).ToDictionaryAsync(t => t.Id, t => $"{t.User.FirstName} {t.User.LastName}");
                model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                return View(model);
            }

            var subject = new Subject { Id = Guid.NewGuid(), Name = model.Name, Description = model.Description, Color = model.Color, TeacherId = model.TeacherId, CreatedAt = DateTime.UtcNow };
            _context.Subjects.Add(subject);
            foreach (var cid in model.ClassroomIds)
                _context.SubjectClassrooms.Add(new SubjectClassroom { SubjectId = subject.Id, ClassroomId = cid });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Matière '{model.Name}' créée avec succès.";
            return RedirectToAction(nameof(Subjects));
        }

        [HttpGet]
        public async Task<IActionResult> EditSubject(Guid id)
        {
            var subject = await _context.Subjects.Include(s => s.SubjectClassrooms).FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound();

            var model = new EditSubjectViewModel
            {
                Id = subject.Id, Name = subject.Name, Description = subject.Description,
                Color = subject.Color, TeacherId = subject.TeacherId,
                ClassroomIds = subject.SubjectClassrooms.Select(sc => sc.ClassroomId).ToList(),
                AvailableTeachers = await _context.Teachers.Include(t => t.User).OrderBy(t => t.User.LastName).ToDictionaryAsync(t => t.Id, t => $"{t.User.FirstName} {t.User.LastName}"),
                AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name)
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubject(EditSubjectViewModel model)
        {
            if (!ModelState.IsValid || !model.ClassroomIds.Any())
            {
                ModelState.AddModelError("ClassroomIds", "Sélectionnez au moins une classe.");
                model.AvailableTeachers = await _context.Teachers.Include(t => t.User).OrderBy(t => t.User.LastName).ToDictionaryAsync(t => t.Id, t => $"{t.User.FirstName} {t.User.LastName}");
                model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                return View(model);
            }

            var subject = await _context.Subjects.Include(s => s.SubjectClassrooms).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (subject == null) return NotFound();

            subject.Name = model.Name;
            subject.Description = model.Description;
            subject.Color = model.Color;
            subject.TeacherId = model.TeacherId;

            _context.SubjectClassrooms.RemoveRange(subject.SubjectClassrooms);
            foreach (var cid in model.ClassroomIds)
                _context.SubjectClassrooms.Add(new SubjectClassroom { SubjectId = subject.Id, ClassroomId = cid });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Matière '{subject.Name}' modifiée avec succès.";
            return RedirectToAction(nameof(Subjects));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubject(Guid id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) { TempData["Error"] = "Matière introuvable."; return RedirectToAction(nameof(Subjects)); }
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Matière supprimée avec succès.";
            return RedirectToAction(nameof(Subjects));
        }

        // ?????????????????????????????????????????????
        // ÉTUDIANTS
        // ?????????????????????????????????????????????
        public async Task<IActionResult> Students(int page = 1, Guid? classroomId = null)
        {
            const int pageSize = 10;
            var query = _context.Students.Include(s => s.User).Include(s => s.Classroom).AsQueryable();
            if (classroomId.HasValue) query = query.Where(s => s.ClassroomId == classroomId.Value);

            var students = await query.OrderBy(s => s.User.LastName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
            ViewBag.Classrooms = await _context.Classrooms.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedClassroom = classroomId;
            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStudent()
        {
            var model = new CreateStudentViewModel
            {
                AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name)
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(CreateStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                return View(model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                return View(model);
            }

            var studentCount = await _context.Students.CountAsync();
            var user = new User
            {
                Id = Guid.NewGuid(), FirstName = model.FirstName, LastName = model.LastName,
                Email = model.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = Role.Student, CreatedAt = DateTime.UtcNow, IsActive = true
            };
            _context.Users.Add(user);
            _context.Students.Add(new Student { Id = Guid.NewGuid(), UserId = user.Id, StudentNumber = $"STU-{DateTime.UtcNow.Year}-{(studentCount + 1):D3}", ClassroomId = model.ClassroomId });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Étudiant {model.FirstName} {model.LastName} créé avec succès.";
            return RedirectToAction(nameof(Students));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) { TempData["Error"] = "Étudiant introuvable."; return RedirectToAction(nameof(Students)); }
            _context.Users.Remove(student.User);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Étudiant supprimé avec succès.";
            return RedirectToAction(nameof(Students));
        }

        // ?????????????????????????????????????????????
        // ÉDITION UTILISATEUR (admin, enseignant, étudiant)
        // ?????????????????????????????????????????????
        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var user = await _context.Users.Include(u => u.Teacher).Include(u => u.Student).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                UserId = user.Id, FirstName = user.FirstName, LastName = user.LastName,
                Email = user.Email, Role = user.Role, IsActive = user.IsActive,
                Department = user.Teacher?.Department, Bio = user.Teacher?.Bio,
                ClassroomId = user.Student?.ClassroomId
            };
            if (user.Role == Role.Student)
                model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!string.IsNullOrEmpty(model.NewPassword) && model.NewPassword != model.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Les mots de passe ne correspondent pas.");

            if (!ModelState.IsValid)
            {
                if (model.Role == Role.Student)
                    model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                return View(model);
            }

            var user = await _context.Users.Include(u => u.Teacher).Include(u => u.Student).FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.UserId))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                if (model.Role == Role.Student)
                    model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.IsActive = model.IsActive;
            if (!string.IsNullOrEmpty(model.NewPassword))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            if (user.Teacher != null) { user.Teacher.Department = model.Department; user.Teacher.Bio = model.Bio; }
            if (user.Student != null && model.ClassroomId.HasValue) user.Student.ClassroomId = model.ClassroomId.Value;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Profil de {user.FirstName} {user.LastName} mis à jour.";
            return RedirectToAction(nameof(EditUser), new { id = model.UserId });
        }
    }
}
