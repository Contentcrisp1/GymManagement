namespace GymManagement.Models;

public class ClassBooking
{
    public int Id { get; set; }

    public int GymClassId { get; set; }
    public GymClass GymClass { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    
    // Trainer can mark whether the member attended the class
    public bool Attended { get; set; } = false;
}