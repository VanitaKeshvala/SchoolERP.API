using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;

namespace SchoolERP.Net.Controllers;

/// <summary>
/// This controller handles general pages like the home page, privacy policy, and error messages.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Shows the main home page of the website.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Shows the privacy policy page, which explains how your data is handled.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Shows a special error page if something goes wrong with the application, helping you report the issue.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Activity.Current?.Id retrieves the active diagnostic correlation ID.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
