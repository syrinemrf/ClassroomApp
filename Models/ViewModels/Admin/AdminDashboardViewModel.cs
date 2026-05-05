namespace ClassroomApp.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalClassrooms { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public List<RecentActivityItem> RecentActivity { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
