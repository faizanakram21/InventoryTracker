using InventoryTracker.Web.Data;
using InventoryTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryTracker.Web.Services;

public class StockService : IStockService
{
    private readonly InventoryDbContext _db;

    public StockService(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetAllProductsWithStockAsync(string? search = null)
    {
        var query = _db.Products
            .Include(p => p.Movements)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Sku.ToLower().Contains(term) ||
                p.Name.ToLower().Contains(term));
        }

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetProductWithMovementsAsync(int id)
    {
        return await _db.Products
            .Include(p => p.Movements.OrderByDescending(m => m.CreatedUtc))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<StockOperationResult> CreateProductAsync(Product product)
    {
        var skuExists = await _db.Products.AnyAsync(p => p.Sku.ToLower() == product.Sku.ToLower());
        if (skuExists)
            return StockOperationResult.Fail($"A product with SKU '{product.Sku}' already exists.");

        product.CreatedUtc = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return StockOperationResult.Ok();
    }

    public async Task<StockOperationResult> UpdateProductAsync(Product product)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing is null)
            return StockOperationResult.Fail("Product not found.");

        var skuTaken = await _db.Products
            .AnyAsync(p => p.Id != product.Id && p.Sku.ToLower() == product.Sku.ToLower());
        if (skuTaken)
            return StockOperationResult.Fail($"A product with SKU '{product.Sku}' already exists.");

        existing.Sku = product.Sku;
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.ReorderLevel = product.ReorderLevel;

        await _db.SaveChangesAsync();
        return StockOperationResult.Ok();
    }

    public async Task<StockOperationResult> DeactivateProductAsync(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return StockOperationResult.Fail("Product not found.");

        product.IsActive = false;
        await _db.SaveChangesAsync();
        return StockOperationResult.Ok();
    }

    public async Task<int> GetCurrentStockAsync(int productId)
    {
        var inSum = await _db.StockMovements
            .Where(m => m.ProductId == productId && m.Type == StockMovementType.In)
            .SumAsync(m => (int?)m.Quantity) ?? 0;

        var outSum = await _db.StockMovements
            .Where(m => m.ProductId == productId && m.Type == StockMovementType.Out)
            .SumAsync(m => (int?)m.Quantity) ?? 0;

        return inSum - outSum;
    }

    // THE KEY RULE: an "Out" movement must never drop stock below zero.
    // We wrap the check + insert in a serializable-ish transaction so two
    // near-simultaneous "Out" requests can't both pass the check and
    // overdraw stock (classic race condition / lost-update problem).
    public async Task<StockOperationResult> RecordMovementAsync(
          int productId, StockMovementType type, int quantity, string? note)
    {
        if (quantity <= 0)
            return StockOperationResult.Fail("Quantity must be a positive number.");

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null)
            return StockOperationResult.Fail("Product not found.");

        // The in-memory EF Core provider (used in unit tests) doesn't support
        // transactions, so we only wrap this in a real transaction when the
        // underlying provider is relational (e.g. Sqlite/SQL Server in production).
        var useTransaction = _db.Database.IsRelational();
        var transaction = useTransaction
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;

        try
        {
            if (type == StockMovementType.Out)
            {
                var currentStock = await GetCurrentStockAsync(productId);
                if (quantity > currentStock)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync();

                    return StockOperationResult.Fail(
                        $"Cannot remove {quantity} unit(s) — only {currentStock} available in stock.");
                }
            }

            _db.StockMovements.Add(new StockMovement
            {
                ProductId = productId,
                Type = type,
                Quantity = quantity,
                Note = note,
                CreatedUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            if (transaction != null)
                await transaction.CommitAsync();

            return StockOperationResult.Ok();
        }
        catch (DbUpdateException)
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            return StockOperationResult.Fail("Something went wrong while recording the movement. Please try again.");
        }
        finally
        {
            if (transaction != null)
                await transaction.DisposeAsync();
        }
    }
}