using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public class StoresMapController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}