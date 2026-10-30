using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.ViewModels;

public class CreateUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Member";

    public List<string> Roles { get; set; } = new();
}