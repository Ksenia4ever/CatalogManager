using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Exceptions;
using CatalogManager.Domain.Entities;
using CatalogManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _service.GetAllAsync();

        return View(categories);
    }

    public async Task<IActionResult> Details(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var category = new Category
            {
                Name = model.Name,
                Description = model.Description
            };

            await _service.CreateAsync(category);

            return RedirectToAction(nameof(Index));
        }
        catch (DuplicateCategoryNameException ex)
        {
            ModelState.AddModelError(
                nameof(model.Name), ex.Message);

            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);

            return View(model);
        }
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return View(model);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, CategoryFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var category = new Category
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            };

            var updated =
                await _service.UpdateAsync(category);

            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
        catch (DuplicateCategoryNameException ex)
        {
            ModelState.AddModelError(
                nameof(model.Name), ex.Message);

            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);

            return View(model);
        }
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
            return RedirectToAction(nameof(Index));

        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        ModelState.AddModelError(
            string.Empty,
            "Cannot delete a category that contains products.");

        return View("Delete", category);
    }
}