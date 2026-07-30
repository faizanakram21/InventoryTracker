using InventoryTracker.Web.Models;
using InventoryTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryTracker.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IStockService _stockService;

    public ProductsController(IStockService stockService)
    {
        _stockService = stockService;
    }

  
    public async Task<IActionResult> Index(string? search)
    {
        var products = await _stockService.GetAllProductsWithStockAsync(search);
        ViewData["Search"] = search;
        return View(products);
    }

 
    public async Task<IActionResult> Details(int id)
    {
        var product = await _stockService.GetProductWithMovementsAsync(id);
        if (product is null)
            return NotFound();

        return View(product);
    }

   
    public IActionResult Create()
    {
        return View(new Product());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
            return View(product);

        var result = await _stockService.CreateProductAsync(product);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(Product.Sku), result.ErrorMessage!);
            return View(product);
        }

        TempData["Success"] = $"Product '{product.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(int id)
    {
        var product = await _stockService.GetProductWithMovementsAsync(id);
        if (product is null)
            return NotFound();

        return View(product);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(product);

        var result = await _stockService.UpdateProductAsync(product);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(Product.Sku), result.ErrorMessage!);
            return View(product);
        }

        TempData["Success"] = $"Product '{product.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _stockService.DeactivateProductAsync(id);
        if (!result.Success)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "Product deactivated.";

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordMovement(int productId, StockMovementType type, int quantity, string? note)
    {
        var result = await _stockService.RecordMovementAsync(productId, type, quantity, note);

        if (!result.Success)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "Movement recorded successfully.";

        return RedirectToAction(nameof(Details), new { id = productId });
    }
}