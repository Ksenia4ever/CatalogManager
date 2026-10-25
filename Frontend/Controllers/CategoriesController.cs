
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: /Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();

        return View(categories);
    }

    // GET: /Categories/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // GET: /Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        ModelState.Remove(nameof(Category.Products));

        if (!ModelState.IsValid)
            return View(category);

        try
        {
            await _categoryService.CreateAsync(category);

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(Category.Name), ex.Message);
            return View(category);
        }
    }

    // GET: /Categories/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: /Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
            return BadRequest();

        ModelState.Remove(nameof(Category.Products));

        if (!ModelState.IsValid)
            return View(category);

        try
        {
            var updated = await _categoryService.UpdateAsync(category);

            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(Category.Name), ex.Message);
            return View(category);
        }
    }

    // GET: /Categories/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: /Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);

        if (!deleted)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            ModelState.AddModelError(
                string.Empty,
                "Cannot delete a category that contains products.");

            return View("Delete", category);
        }

        return RedirectToAction(nameof(Index));
    }
}
