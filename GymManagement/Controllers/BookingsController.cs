using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Models.ViewModels;
using GymManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers;

// Members can book classes.
// Admin is also allowed here for testing and administrative verification.
[Authorize(Roles = "Member,Admin")]
public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public BookingsController(
        IBookingService bookingService,
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _bookingService = bookingService;
        _context = context;
        _userManager = userManager;
    }

    // GET: /BookClasses  
    [HttpGet]
    [Route("BookClasses")]
    [Route("Bookings")]
    public async Task<IActionResult> Index()
    {
        var member = await GetCurrentMemberAsync();
        if (member is null) return NoMemberProfile();

        var upcomingClasses = await _bookingService.GetUpcomingClassesAsync();

        var model = upcomingClasses
            .Select(c => new UpcomingClassViewModel
            {
                Id = c.Id,
                Name = c.Name,
                TrainerName = c.Trainer is not null
                    ? $"{c.Trainer.FirstName} {c.Trainer.LastName}"
                    : "Unassigned",
                StartTime = c.StartTime,
                DurationInMinutes = c.DurationInMinutes,
                MaxCapacity = c.MaxCapacity,
                BookedCount = c.Bookings.Count,
                AlreadyBooked = c.Bookings.Any(b => b.MemberId == member.Id)
            })
            .ToList();

        return View(model);
    }

    // POST: /Bookings/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int id)
    {
        var member = await GetCurrentMemberAsync();
        if (member is null) return NoMemberProfile();

        var (success, message) = await _bookingService.BookClassAsync(member.Id, id);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /MyBookings 
    [HttpGet]
    [Route("MyBookings")]
    [Route("Bookings/MyBookings")]
    public async Task<IActionResult> MyBookings()
    {
        var member = await GetCurrentMemberAsync();
        if (member is null) return NoMemberProfile();

        var bookings = await _bookingService.GetMemberBookingsAsync(member.Id);

        return View(bookings);
    }

    // POST: /Bookings/Cancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var member = await GetCurrentMemberAsync();
        if (member is null) return NoMemberProfile();

        var cancelled = await _bookingService.CancelBookingAsync(id, member.Id);

        if (cancelled)
        {
            TempData["SuccessMessage"] = "Your booking has been cancelled.";
        }
        else
        {
            TempData["ErrorMessage"] = "That booking could not be found.";
        }

        return RedirectToAction(nameof(MyBookings));
    }


    private async Task<Member?> GetCurrentMemberAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        return await _context.Members
            .FirstOrDefaultAsync(m => m.IdentityUserId == userId);
    }

    private IActionResult NoMemberProfile()
    {
        TempData["ErrorMessage"] =
            "No member profile is linked to your account. Please contact the gym staff.";
        return RedirectToAction("Index", "Home");
    }
}
