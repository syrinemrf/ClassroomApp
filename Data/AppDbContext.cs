using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClassroomApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<SubjectClassroom> SubjectClassrooms { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseClassroom> CourseClassrooms { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User ? unique email index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            // User ? Teacher (1-to-1)
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ? Student (1-to-1)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student ? Classroom (many students per classroom)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Classroom)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ? Teacher
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ? Subject
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Subject)
                .WithMany(s => s.Courses)
                .HasForeignKey(c => c.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subject ? Teacher
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Teacher)
                .WithMany(t => t.Subjects)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubjectClassroom (many-to-many Subject ? Classroom)
            modelBuilder.Entity<SubjectClassroom>()
                .HasKey(sc => new { sc.SubjectId, sc.ClassroomId });
            modelBuilder.Entity<SubjectClassroom>()
                .HasOne(sc => sc.Subject)
                .WithMany(s => s.SubjectClassrooms)
                .HasForeignKey(sc => sc.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SubjectClassroom>()
                .HasOne(sc => sc.Classroom)
                .WithMany(cl => cl.SubjectClassrooms)
                .HasForeignKey(sc => sc.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assignment ? Subject (optional)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Subject)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // CourseClassroom (many-to-many Course ? Classroom)
            modelBuilder.Entity<CourseClassroom>()
                .HasKey(cc => new { cc.CourseId, cc.ClassroomId });
            modelBuilder.Entity<CourseClassroom>()
                .HasOne(cc => cc.Course)
                .WithMany(c => c.CourseClassrooms)
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CourseClassroom>()
                .HasOne(cc => cc.Classroom)
                .WithMany(cl => cl.CourseClassrooms)
                .HasForeignKey(cc => cc.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assignment ? Teacher
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignment ? Classroom
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Classroom)
                .WithMany(cl => cl.Assignments)
                .HasForeignKey(a => a.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission ? Student
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany(st => st.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Submission ? Assignment
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission ? GradedBy Teacher
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.GradedBy)
                .WithMany(t => t.GradedSubmissions)
                .HasForeignKey(s => s.GradedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment ? Assignment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Assignment)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment ? User
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment ? ParentComment (self-referencing)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification ? User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Message ? Sender
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message ? Receiver
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // CalendarEvent ? CreatedBy
            modelBuilder.Entity<CalendarEvent>()
                .HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedCalendarEvents)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CalendarEvent ? Classroom
            modelBuilder.Entity<CalendarEvent>()
                .HasOne(e => e.Classroom)
                .WithMany(c => c.CalendarEvents)
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Fixed GUIDs for stable migrations
            var adminUserId = new Guid("11111111-1111-1111-1111-111111111111");

            var teacher1UserId = new Guid("22222222-2222-2222-2222-222222222221");
            var teacher1Id = new Guid("22222222-2222-2222-2222-222222222222");
            var teacher2UserId = new Guid("33333333-3333-3333-3333-333333333331");
            var teacher2Id = new Guid("33333333-3333-3333-3333-333333333332");

            var classroom1Id = new Guid("44444444-4444-4444-4444-444444444441");
            var classroom2Id = new Guid("44444444-4444-4444-4444-444444444442");

            var student1UserId = new Guid("55555555-5555-5555-5555-555555555551");
            var student1Id = new Guid("55555555-5555-5555-5555-555555555561");
            var student2UserId = new Guid("55555555-5555-5555-5555-555555555552");
            var student2Id = new Guid("55555555-5555-5555-5555-555555555562");
            var student3UserId = new Guid("55555555-5555-5555-5555-555555555553");
            var student3Id = new Guid("55555555-5555-5555-5555-555555555563");
            var student4UserId = new Guid("55555555-5555-5555-5555-555555555554");
            var student4Id = new Guid("55555555-5555-5555-5555-555555555564");
            var student5UserId = new Guid("55555555-5555-5555-5555-555555555555");
            var student5Id = new Guid("55555555-5555-5555-5555-555555555565");

            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // ?? Admin User ??
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminUserId,
                FirstName = "Super",
                LastName = "Admin",
                Email = "admin@classroom.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = Role.Admin,
                CreatedAt = seedDate,
                IsActive = true
            });

            // ?? Teacher Users ??
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = teacher1UserId,
                FirstName = "Ahmed",
                LastName = "Benali",
                Email = "ahmed.benali@classroom.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                Role = Role.Teacher,
                CreatedAt = seedDate,
                IsActive = true
            });
            modelBuilder.Entity<Teacher>().HasData(new Teacher
            {
                Id = teacher1Id,
                UserId = teacher1UserId,
                Department = "Computer Science",
                Bio = "Senior lecturer in Software Engineering with 10 years of experience."
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = teacher2UserId,
                FirstName = "Fatima",
                LastName = "Zahra",
                Email = "fatima.zahra@classroom.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                Role = Role.Teacher,
                CreatedAt = seedDate,
                IsActive = true
            });
            modelBuilder.Entity<Teacher>().HasData(new Teacher
            {
                Id = teacher2Id,
                UserId = teacher2UserId,
                Department = "Mathematics",
                Bio = "PhD in Applied Mathematics, passionate about data science."
            });

            // ?? Classrooms ??
            modelBuilder.Entity<Classroom>().HasData(new Classroom
            {
                Id = classroom1Id,
                Name = "CS-101",
                Description = "Introduction to Computer Science - Section A",
                AcademicYear = "2024-2025"
            });
            modelBuilder.Entity<Classroom>().HasData(new Classroom
            {
                Id = classroom2Id,
                Name = "MATH-201",
                Description = "Advanced Mathematics - Section B",
                AcademicYear = "2024-2025"
            });

            // ?? Student Users ??
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = student1UserId,
                FirstName = "Youssef",
                LastName = "Amrani",
                Email = "youssef.amrani@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role = Role.Student,
                CreatedAt = seedDate,
                IsActive = true
            });
            modelBuilder.Entity<Student>().HasData(new Student
            {
                Id = student1Id,
                UserId = student1UserId,
                StudentNumber = "STU-2024-001",
                ClassroomId = classroom1Id
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = student2UserId,
                FirstName = "Sara",
                LastName = "Elkadi",
                Email = "sara.elkadi@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role = Role.Student,
                CreatedAt = seedDate,
                IsActive = true
            });
            modelBuilder.Entity<Student>().HasData(new Student
            {
                Id = student2Id,
                UserId = student2UserId,
                StudentNumber = "STU-2024-002",
                ClassroomId = classroom1Id
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = student3UserId,
                FirstName = "Karim",
                LastName = "Ouhadi",
                Email = "karim.ouhadi@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role = Role.Student,
                CreatedAt = seedDate,
                IsActive = true
            });
            modelBuilder.Entity<Student>().HasData(new Student
            {
                Id = student3Id,
                UserId = student3UserId,
                StudentNumber = "STU-2024-003",
                ClassroomId = classroom1Id
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = student4UserId,
                FirstName = "Leila",
                LastName = "Mansouri",
                Email = "leila.mansouri@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role = Role.Student,
                CreatedAt = seedDate,
                IsActive = true
            });
            modelBuilder.Entity<Student>().HasData(new Student
            {
                Id = student4Id,
                UserId = student4UserId,
                StudentNumber = "STU-2024-004",
                ClassroomId = classroom2Id
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = student5UserId,
                FirstName = "Omar",
                LastName = "Tazi",
                Email = "omar.tazi@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role = Role.Student,
                CreatedAt = seedDate,
                IsActive = true
            });
            modelBuilder.Entity<Student>().HasData(new Student
            {
                Id = student5Id,
                UserId = student5UserId,
                StudentNumber = "STU-2024-005",
                ClassroomId = classroom2Id
            });
        }
    }
}
