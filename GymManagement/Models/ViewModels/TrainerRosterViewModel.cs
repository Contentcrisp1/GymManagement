namespace GymManagement.Models.ViewModels;

public class TrainerRosterViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public int DurationInMinutes { get; set; }

    public int MaxCapacity { get; set; }

    public string TrainerName { get; set; } = string.Empty;

    public string TrainerSpecialization { get; set; } = string.Empty;

    public List<TrainerRosterMemberViewModel> Members { get; set; } = new();
}

public class TrainerRosterMemberViewModel
{
    public int BookingId { get; set; }

    public int MemberId { get; set; }

    public string MemberName { get; set; } = string.Empty;

    public DateTime BookedAt { get; set; }

    public bool Attended { get; set; }
}