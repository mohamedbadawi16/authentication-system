using System.Diagnostics;
using AuthenticationSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AuthenticationSystem.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var token = HttpContext.Session.GetString("auth:token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewData["Email"] = HttpContext.Session.GetString("auth:email") ?? string.Empty;
        ViewData["ExpiresAt"] = HttpContext.Session.GetString("auth:expires") ?? string.Empty;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "Logged out.";
        return RedirectToAction("Login", "Auth");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
