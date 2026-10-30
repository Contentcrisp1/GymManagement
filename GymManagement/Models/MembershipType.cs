using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models;

public class MembershipType
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty; // e.g., "Monthly Pass"

    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int DurationInDays { get; set; } // e.g., 30, 90, 365

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}