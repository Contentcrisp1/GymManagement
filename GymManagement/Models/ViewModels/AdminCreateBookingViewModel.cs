
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.ViewModels;

public class AdminCreateBookingViewModel
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public int GymClassId { get; set; }

    public List<SelectListItem> Members { get; set; } = new();

    public List<SelectListItem> Classes { get; set; } = new();
}