using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Models;

public class Member
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    // Optional link to ASP.NET Core Identity User
    public string? IdentityUserId { get; set; }
    public IdentityUser? IdentityUser { get; set; }

    // Navigation Properties
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
}