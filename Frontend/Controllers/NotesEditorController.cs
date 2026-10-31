using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public sealed class NotesEditorController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}