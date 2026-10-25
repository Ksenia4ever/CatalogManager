
using CatalogManager.Application.Interfaces;
using CatalogManager.Domain.Entities;
using CatalogManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

public class StoresController : Controller
{
    private readonly IStoreService _service;

    public StoresController(IStoreService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var stores = await _service.GetAllAsync();
        return View(stores);
    }

    public async Task<IActionResult> Details(int id)
    {
        var store = await _service.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        return View(store);
    }

    // GET: Stores/Create
    public IActionResult Create()
    {
        return View(new StoreFormViewModel());
    }

    // POST: Stores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StoreFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var store = new Store
            {
                Name = model.Name,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            await _service.CreateAsync(store);

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET: Stores/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var store = await _service.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        var model = new StoreFormViewModel
        {
            Id = store.Id,
            Name = store.Name,
            Address = store.Address,
            Latitude = store.Latitude,
            Longitude = store.Longitude
        };

        return View(model);
    }

    // POST: Stores/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, StoreFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var store = new Store
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            var updated = await _service.UpdateAsync(store);

            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET: Stores/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var store = await _service.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        return View(store);
    }

    // POST: Stores/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
            return RedirectToAction(nameof(Index));

        var store = await _service.GetByIdAsync(id);

        if (store == null)
            return NotFound();

        ModelState.AddModelError(
            string.Empty,
            "Cannot delete a store that contains products.");

        return View("Delete", store);
    }
}
