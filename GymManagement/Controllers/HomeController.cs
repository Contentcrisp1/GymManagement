using System.Diagnostics;
using GymManagement.Data;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Public page - guests can see the membership plans.
        var plans = await _context.MembershipTypes
            .ToListAsync();

        // SQLite does not support decimal in ORDER BY,
        // so order the results after they are loaded.
        plans = plans
            .OrderBy(p => p.Price)
            .ToList();

        return View(plans);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
        });
    }
}