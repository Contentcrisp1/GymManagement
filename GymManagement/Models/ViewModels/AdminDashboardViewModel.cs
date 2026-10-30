namespace GymManagement.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalMembers { get; set; }

    public int ActiveSubscriptions { get; set; }

    public int TotalClasses { get; set; }

    public int TodayBookings { get; set; }
}