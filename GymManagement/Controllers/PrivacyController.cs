using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers;

public class PrivacyController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}