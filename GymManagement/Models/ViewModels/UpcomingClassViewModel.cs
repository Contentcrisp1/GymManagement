namespace GymManagement.Models.ViewModels;

public class UpcomingClassViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TrainerName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public int DurationInMinutes { get; set; }

    public int MaxCapacity { get; set; }

    public int BookedCount { get; set; }

    /// <summary>How many places are still available (never negative).</summary>
    public int SpotsLeft => Math.Max(0, MaxCapacity - BookedCount);

    /// <summary>True when the class has reached its maximum capacity.</summary>
    public bool IsFull => BookedCount >= MaxCapacity;

    /// <summary>True when the signed-in member already holds a booking for this class.</summary>
    public bool AlreadyBooked { get; set; }
}
