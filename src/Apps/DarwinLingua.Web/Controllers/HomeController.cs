using System.Diagnostics;
using DarwinLingua.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet("")]
    [OutputCache(PolicyName = "LandingPage")]
    [ActionName("Index")]
    [Route("", Name = "Home_Index")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("install", Name = "Home_Install")]
    public IActionResult Install()
    {
        ViewData["Title"] = "Install";
        return View();
    }

    [HttpGet("error", Name = "Home_Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
