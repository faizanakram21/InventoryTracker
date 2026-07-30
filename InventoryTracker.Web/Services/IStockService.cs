using InventoryTracker.Web.Models;

namespace InventoryTracker.Web.Services;

public class StockOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static StockOperationResult Ok() => new() { Success = true };
    public static StockOperationResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}

public interface IStockService
{
    Task<List<Product>> GetAllProductsWithStockAsync(string? search = null);
    Task<Product?> GetProductWithMovementsAsync(int id);
    Task<StockOperationResult> CreateProductAsync(Product product);
    Task<StockOperationResult> UpdateProductAsync(Product product);
    Task<StockOperationResult> DeactivateProductAsync(int id);
    Task<StockOperationResult> RecordMovementAsync(int productId, StockMovementType type, int quantity, string? note);
    Task<int> GetCurrentStockAsync(int productId);
}