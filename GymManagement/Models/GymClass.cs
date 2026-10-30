using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models;

public class GymClass
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Morning Yoga"

    public DateTime StartTime { get; set; }
    public int DurationInMinutes { get; set; }
    public int MaxCapacity { get; set; }

    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;

    // Navigation Property
    public ICollection<ClassBooking> Bookings { get; set; } = new List<ClassBooking>();
}