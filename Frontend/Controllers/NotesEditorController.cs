using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public sealed class NotesEditorController : Controller
{
    [HttpGet]
    public IActionResult Index(string documentId = "shared")
    {
        ViewBag.DocumentId = documentId;
        return View();
    }
}