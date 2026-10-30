using GymManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: /Users
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();

        var model = new List<AdminUserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Add(new AdminUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        return View(model);
    }

    // GET: /Users/Create
    public async Task<IActionResult> Create()
    {
        var model = new CreateUserViewModel
        {
            Roles = await _roleManager.Roles
                .Where(x => x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync()
        };

        return View(model);
    }

    // POST: /Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = await GetRolesAsync();
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "A user with this email already exists.");

            model.Roles = await GetRolesAsync();

            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Roles = await GetRolesAsync();

            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.Role))
        {
            var roleExists = await _roleManager.RoleExistsAsync(model.Role);

            if (roleExists)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Users/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (id == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            TempData["Error"] = "The user could not be deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetRolesAsync()
    {
        return await _roleManager.Roles
            .Where(x => x.Name != null)
            .Select(x => x.Name!)
            .ToListAsync();
    }
}