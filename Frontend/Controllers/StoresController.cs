
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public class StoresController : Controller
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    // GET: /Stores
    public async Task<IActionResult> Index()
    {
        var stores = await _storeService.GetAllAsync();
        return View(stores);
    }

    // GET: /Stores/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var store = await _storeService.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        return View(store);
    }

    // GET: /Stores/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Stores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Store store)
    {
        ModelState.Remove(nameof(Store.Products));

        if (!ModelState.IsValid)
            return View(store);

        try
        {
            await _storeService.CreateAsync(store);
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(store);
        }
    }

    // GET: /Stores/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var store = await _storeService.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        return View(store);
    }

    // POST: /Stores/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Store store)
    {
        if (id != store.Id)
            return BadRequest();

        ModelState.Remove(nameof(Store.Products));

        if (!ModelState.IsValid)
            return View(store);

        try
        {
            var updated = await _storeService.UpdateAsync(store);

            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(store);
        }
    }

    // GET: /Stores/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var store = await _storeService.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        return View(store);
    }

    // POST: /Stores/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _storeService.DeleteAsync(id);

        if (!deleted)
        {
            var store = await _storeService.GetByIdAsync(id);

            if (store == null)
                return NotFound();

            ModelState.AddModelError(
                string.Empty,
                "Cannot delete a store that contains products.");

            return View("Delete", store);
        }

        return RedirectToAction(nameof(Index));
    }
}
