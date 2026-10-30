using GymManagement.Data;
using GymManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers;

[Authorize]
public class MembershipsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public MembershipsController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // =========================================================
    // MEMBER - VIEW MEMBERSHIP PLANS
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.MembershipTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search));
        }

        var memberships = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.Search = search;

        return View(memberships);
    }

    // =========================================================
    // MEMBER - SUBSCRIBE PAGE
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Subscribe(int id)
    {
        var membership = await _context.MembershipTypes
            .FirstOrDefaultAsync(x => x.Id == id);

        if (membership == null)
        {
            return NotFound();
        }

        return View(membership);
    }

    // =========================================================
    // MEMBER - CONFIRM SUBSCRIPTION
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubscribeConfirmed(int id)
    {
        var membership = await _context.MembershipTypes
            .FirstOrDefaultAsync(x => x.Id == id);

        if (membership == null)
        {
            return NotFound("Membership plan not found.");
        }

        var identityUserId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(identityUserId))
        {
            return Challenge();
        }

        var identityUser = await _userManager.FindByIdAsync(identityUserId);

        if (identityUser == null)
        {
            return Unauthorized();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId);

        if (member == null)
        {
            member = new Member
            {
                IdentityUserId = identityUserId,
                FirstName = "New",
                LastName = "Member",
                JoinedDate = DateTime.UtcNow
            };

            _context.Members.Add(member);

            await _context.SaveChangesAsync();
        }

        var memberExists = await _context.Members
            .AnyAsync(x => x.Id == member.Id);

        if (!memberExists)
        {
            return BadRequest("Member record could not be created.");
        }

        var membershipExists = await _context.MembershipTypes
            .AnyAsync(x => x.Id == membership.Id);

        if (!membershipExists)
        {
            return BadRequest("Membership plan no longer exists.");
        }

        var now = DateTime.UtcNow;

        var currentSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(x =>
                x.MemberId == member.Id &&
                x.StartDate <= now &&
                x.EndDate >= now);

        if (currentSubscription != null)
        {
            _context.Subscriptions.Remove(currentSubscription);

            await _context.SaveChangesAsync();
        }

        var subscription = new Subscription
        {
            MemberId = member.Id,
            MembershipTypeId = membership.Id,
            StartDate = now,
            EndDate = now.AddDays(membership.DurationInDays)
        };

        _context.Subscriptions.Add(subscription);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MySubscription));
    }

    // =========================================================
    // MEMBER - MY SUBSCRIPTION
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> MySubscription()
    {
        var identityUserId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(identityUserId))
        {
            return Challenge();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId);

        if (member == null)
        {
            return View(null);
        }

        var subscription = await _context.Subscriptions
            .Include(x => x.MembershipType)
            .Where(x => x.MemberId == member.Id)
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefaultAsync();

        return View(subscription);
    }

    // =========================================================
    // ADMIN - MANAGE MEMBERSHIPS
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Manage(string? search)
    {
        var query = _context.MembershipTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search));
        }

        var memberships = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.Search = search;

        return View(memberships);
    }

    // =========================================================
    // ADMIN - CREATE MEMBERSHIP
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MembershipType membership)
    {
        if (!ModelState.IsValid)
        {
            var memberships = await _context.MembershipTypes
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.Search = null;

            return View("Manage", memberships);
        }

        var nameExists = await _context.MembershipTypes
            .AnyAsync(x =>
                x.Name.ToLower() == membership.Name.ToLower());

        if (nameExists)
        {
            ModelState.AddModelError(
                "Name",
                "A membership plan with this name already exists.");

            var memberships = await _context.MembershipTypes
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View("Manage", memberships);
        }

        _context.MembershipTypes.Add(membership);

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Membership plan created successfully.";

        return RedirectToAction(nameof(Manage));
    }

    // =========================================================
    // ADMIN - EDIT MEMBERSHIP PAGE
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var membership = await _context.MembershipTypes
            .FindAsync(id);

        if (membership == null)
        {
            return NotFound();
        }

        return View(membership);
    }

    // =========================================================
    // ADMIN - EDIT MEMBERSHIP
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        MembershipType membership)
    {
        if (id != membership.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var memberships = await _context.MembershipTypes
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.Search = null;

            return View("Manage", memberships);
        }

        var existingMembership =
            await _context.MembershipTypes.FindAsync(id);

        if (existingMembership == null)
        {
            return NotFound();
        }

        var nameExists = await _context.MembershipTypes
            .AnyAsync(x =>
                x.Id != id &&
                x.Name.ToLower() == membership.Name.ToLower());

        if (nameExists)
        {
            TempData["Error"] =
                "Another membership plan already uses this name.";

            return RedirectToAction(nameof(Manage));
        }

        existingMembership.Name = membership.Name;
        existingMembership.Description = membership.Description;
        existingMembership.Price = membership.Price;
        existingMembership.DurationInDays =
            membership.DurationInDays;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Membership plan updated successfully.";

        return RedirectToAction(nameof(Manage));
    }

    // =========================================================
    // ADMIN - DELETE MEMBERSHIP CONFIRMATION PAGE
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var membership = await _context.MembershipTypes
            .FindAsync(id);

        if (membership == null)
        {
            return NotFound();
        }

        return View(membership);
    }

    // =========================================================
    // ADMIN - DELETE MEMBERSHIP
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, MembershipType membership)
    {
        var existingMembership =
            await _context.MembershipTypes.FindAsync(id);

        if (existingMembership == null)
        {
            return NotFound();
        }

        var hasSubscriptions = await _context.Subscriptions
            .AnyAsync(x => x.MembershipTypeId == id);

        if (hasSubscriptions)
        {
            TempData["Error"] =
                "This membership plan cannot be deleted because it has existing subscriptions.";

            return RedirectToAction(nameof(Manage));
        }

        _context.MembershipTypes.Remove(existingMembership);

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Membership plan deleted successfully.";

        return RedirectToAction(nameof(Manage));
    }
}