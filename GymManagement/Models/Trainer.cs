using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Models;

public class Trainer
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(100)]
    public string Specialization { get; set; } = string.Empty; // e.g., "Personal Trainer", "Yoga"

    // Optional link to ASP.NET Core Identity User
    public string? IdentityUserId { get; set; }
    public IdentityUser? IdentityUser { get; set; }

    // Navigation Properties
    public ICollection<GymClass> GymClasses { get; set; } = new List<GymClass>();
}