
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // DASHBOARD
    // =========================================================

    public async Task<IActionResult> Dashboard()
    {
        var model = new AdminDashboardViewModel
        {
            TotalMembers = await _context.Members.CountAsync(),

            ActiveSubscriptions = await _context.Subscriptions
                .CountAsync(x =>
                    DateTime.UtcNow >= x.StartDate &&
                    DateTime.UtcNow <= x.EndDate),

            TotalClasses = await _context.GymClasses.CountAsync(),

            TodayBookings = await _context.ClassBookings
                .CountAsync(x =>
                    x.BookedAt.Date == DateTime.UtcNow.Date)
        };

        return View(model);
    }

    // =========================================================
    // BOOKINGS - LIST
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Bookings()
    {
        var bookings = await _context.ClassBookings
            .Include(x => x.GymClass)
                .ThenInclude(x => x.Trainer)
            .Include(x => x.Member)
            .OrderByDescending(x => x.BookedAt)
            .ToListAsync();

        return View(bookings);
    }

    // =========================================================
    // BOOKINGS - CREATE GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> CreateBooking()
    {
        var model = new AdminCreateBookingViewModel();

        await LoadBookingListsAsync(model);

        return View(model);
    }

    // =========================================================
    // BOOKINGS - CREATE POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBooking(
        AdminCreateBookingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadBookingListsAsync(model);
            return View(model);
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.Id == model.MemberId);

        if (member == null)
        {
            ModelState.AddModelError(
                nameof(model.MemberId),
                "Member was not found.");

            await LoadBookingListsAsync(model);
            return View(model);
        }

        var gymClass = await _context.GymClasses
            .Include(x => x.Bookings)
            .FirstOrDefaultAsync(x => x.Id == model.GymClassId);

        if (gymClass == null)
        {
            ModelState.AddModelError(
                nameof(model.GymClassId),
                "Class was not found.");

            await LoadBookingListsAsync(model);
            return View(model);
        }

        // Check if this member is already booked
        var alreadyBooked = await _context.ClassBookings
            .AnyAsync(x =>
                x.MemberId == model.MemberId &&
                x.GymClassId == model.GymClassId);

        if (alreadyBooked)
        {
            ModelState.AddModelError(
                string.Empty,
                "This member is already booked into this class.");

            await LoadBookingListsAsync(model);
            return View(model);
        }

        // Check capacity
        if (gymClass.Bookings.Count >= gymClass.MaxCapacity)
        {
            ModelState.AddModelError(
                string.Empty,
                "This class is already full.");

            await LoadBookingListsAsync(model);
            return View(model);
        }

        // Check class time
        if (gymClass.StartTime <= DateTime.UtcNow)
        {
            ModelState.AddModelError(
                string.Empty,
                "You cannot book a class that has already started.");

            await LoadBookingListsAsync(model);
            return View(model);
        }

        var booking = new ClassBooking
        {
            MemberId = member.Id,
            GymClassId = gymClass.Id,
            BookedAt = DateTime.UtcNow
        };

        _context.ClassBookings.Add(booking);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Booking created for {member.FirstName} {member.LastName}.";

        return RedirectToAction(nameof(Bookings));
    }

    // =========================================================
    // BOOKINGS - DELETE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _context.ClassBookings
            .Include(x => x.Member)
            .Include(x => x.GymClass)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (booking == null)
        {
            TempData["ErrorMessage"] =
                "Booking could not be found.";

            return RedirectToAction(nameof(Bookings));
        }

        _context.ClassBookings.Remove(booking);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Booking has been cancelled.";

        return RedirectToAction(nameof(Bookings));
    }

    // =========================================================
    // DROPDOWN DATA
    // =========================================================

    private async Task LoadBookingListsAsync(
        AdminCreateBookingViewModel model)
    {
        model.Members = await _context.Members
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.FirstName + " " + x.LastName
            })
            .ToListAsync();

        model.Classes = await _context.GymClasses
            .Include(x => x.Trainer)
            .Where(x => x.StartTime > DateTime.UtcNow)
            .OrderBy(x => x.StartTime)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text =
                    x.Name +
                    " - " +
                    x.StartTime.ToString("dd MMM yyyy HH:mm") +
                    " - " +
                    x.Trainer.FirstName +
                    " " +
                    x.Trainer.LastName
            })
            .ToListAsync();
    }
}

