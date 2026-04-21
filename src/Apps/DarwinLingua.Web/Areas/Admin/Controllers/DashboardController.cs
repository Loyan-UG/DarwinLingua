using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
