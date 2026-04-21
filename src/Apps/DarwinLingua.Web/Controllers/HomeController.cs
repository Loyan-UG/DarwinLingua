using System.Diagnostics;
using DarwinLingua.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

public class HomeController : Controller
{
    [OutputCache(PolicyName = "LandingPage")]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Install()
    {
        ViewData["Title"] = "Install";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
