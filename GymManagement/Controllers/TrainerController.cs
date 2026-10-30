using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers;

[Authorize(Roles = "Trainer")]
public class TrainerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public TrainerController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // =========================================================
    // TRAINER SCHEDULE
    // GET: /Trainer/Schedule
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Schedule()
    {
        var trainer = await GetCurrentTrainerAsync();

        if (trainer == null)
        {
            return NotFound("Trainer profile was not found.");
        }

        var classes = await _context.GymClasses
            .Where(c => c.TrainerId == trainer.Id)
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .OrderBy(c => c.StartTime)
            .ToListAsync();

        return View(classes);
    }

    // =========================================================
    // CREATE CLASS - GET
    // GET: /Trainer/CreateClass
    // =========================================================

    [HttpGet]
    public IActionResult CreateClass()
    {
        var model = new TrainerCreateClassViewModel
        {
            StartTime = DateTime.Now.AddDays(1)
        };

        return View(model);
    }

    // =========================================================
    // CREATE CLASS - POST
    // POST: /Trainer/CreateClass
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClass(
        TrainerCreateClassViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var trainer = await GetCurrentTrainerAsync();

        if (trainer == null)
        {
            return NotFound("Trainer profile was not found.");
        }

        var gymClass = new GymClass
        {
            Name = model.Name,
            StartTime = model.StartTime.ToUniversalTime(),
            DurationInMinutes = model.DurationInMinutes,
            MaxCapacity = model.MaxCapacity,
            TrainerId = trainer.Id
        };

        _context.GymClasses.Add(gymClass);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "The gym class was created successfully.";

        return RedirectToAction(nameof(Schedule));
    }

    // =========================================================
    // CLASS ROSTER
    // GET: /Trainer/Roster/5
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Roster(int id)
    {
        var trainer = await GetCurrentTrainerAsync();

        if (trainer == null)
        {
            return NotFound("Trainer profile was not found.");
        }

        var gymClass = await _context.GymClasses
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Member)
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.TrainerId == trainer.Id);

        if (gymClass == null)
        {
            return NotFound(
                "The class does not exist or is not assigned to you.");
        }

        var model = new TrainerRosterViewModel
        {
            ClassId = gymClass.Id,
            ClassName = gymClass.Name,
            StartTime = gymClass.StartTime,
            DurationInMinutes = gymClass.DurationInMinutes,
            MaxCapacity = gymClass.MaxCapacity,

            TrainerName =
                $"{gymClass.Trainer.FirstName} {gymClass.Trainer.LastName}",

            TrainerSpecialization =
                gymClass.Trainer.Specialization,

            Members = gymClass.Bookings
                .OrderBy(b => b.Member.LastName)
                .ThenBy(b => b.Member.FirstName)
                .Select(b => new TrainerRosterMemberViewModel
                {
                    BookingId = b.Id,
                    MemberId = b.MemberId,

                    MemberName =
                        $"{b.Member.FirstName} {b.Member.LastName}",

                    BookedAt = b.BookedAt,
                    Attended = b.Attended
                })
                .ToList()
        };

        return View(model);
    }

    // =========================================================
    // MARK ATTENDANCE
    // POST: /Trainer/MarkAttendance
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAttendance(
        int classId,
        int[] attendedBookingIds)
    {
        var trainer = await GetCurrentTrainerAsync();

        if (trainer == null)
        {
            return NotFound("Trainer profile was not found.");
        }

        var gymClass = await _context.GymClasses
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c =>
                c.Id == classId &&
                c.TrainerId == trainer.Id);

        if (gymClass == null)
        {
            return NotFound(
                "The class does not exist or is not assigned to you.");
        }

        attendedBookingIds ??= Array.Empty<int>();

        foreach (var booking in gymClass.Bookings)
        {
            booking.Attended =
                attendedBookingIds.Contains(booking.Id);
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Attendance has been updated successfully.";

        return RedirectToAction(
            nameof(Roster),
            new { id = classId });
    }

    // =========================================================
    // GET CURRENT TRAINER
    // =========================================================

    private async Task<Trainer?> GetCurrentTrainerAsync()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _context.Trainers
            .FirstOrDefaultAsync(t =>
                t.IdentityUserId == userId);
    }
}