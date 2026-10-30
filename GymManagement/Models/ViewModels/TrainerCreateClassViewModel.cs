using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.ViewModels;

public class TrainerCreateClassViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Class Title")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Start Time")]
    public DateTime StartTime { get; set; }

    [Required]
    [Range(1, 600)]
    [Display(Name = "Duration (minutes)")]
    public int DurationInMinutes { get; set; }

    [Required]
    [Range(1, 1000)]
    [Display(Name = "Maximum Capacity")]
    public int MaxCapacity { get; set; }
}