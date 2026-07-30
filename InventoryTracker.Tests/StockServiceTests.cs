using InventoryTracker.Web.Data;
using InventoryTracker.Web.Models;
using InventoryTracker.Web.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryTracker.Tests;

public class StockServiceTests
{
    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    [Fact]
    public async Task CurrentStock_IsSumOfIn_MinusSumOfOut()
    {
        // Arrange
        using var db = CreateContext();
        var service = new StockService(db);

        var product = new Product { Sku = "TST-001", Name = "Test Widget" };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Act
        await service.RecordMovementAsync(product.Id, StockMovementType.In, 50, "Initial");
        await service.RecordMovementAsync(product.Id, StockMovementType.In, 20, "Restock");
        await service.RecordMovementAsync(product.Id, StockMovementType.Out, 30, "Sold");

        var stock = await service.GetCurrentStockAsync(product.Id);

        // Assert
        Assert.Equal(40, stock); // 50 + 20 - 30
    }

    [Fact]
    public async Task RecordMovement_OutExceedingStock_IsRejected()
    {
        // Arrange
        using var db = CreateContext();
        var service = new StockService(db);

        var product = new Product { Sku = "TST-002", Name = "Test Widget 2" };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        await service.RecordMovementAsync(product.Id, StockMovementType.In, 10, "Initial");

        // Act — try to remove more than available
        var result = await service.RecordMovementAsync(product.Id, StockMovementType.Out, 15, "Too much");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("only 10", result.ErrorMessage);

        var stockAfter = await service.GetCurrentStockAsync(product.Id);
        Assert.Equal(10, stockAfter); // unchanged
    }

    [Fact]
    public async Task RecordMovement_OutExactlyEqualToStock_Succeeds()
    {
        // Arrange
        using var db = CreateContext();
        var service = new StockService(db);

        var product = new Product { Sku = "TST-003", Name = "Test Widget 3" };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        await service.RecordMovementAsync(product.Id, StockMovementType.In, 25, "Initial");

        // Act — remove exactly what's available (stock should reach zero, not negative)
        var result = await service.RecordMovementAsync(product.Id, StockMovementType.Out, 25, "Clear out");

        // Assert
        Assert.True(result.Success);
        var stockAfter = await service.GetCurrentStockAsync(product.Id);
        Assert.Equal(0, stockAfter);
    }

    [Fact]
    public async Task RecordMovement_NegativeOrZeroQuantity_IsRejected()
    {
        // Arrange
        using var db = CreateContext();
        var service = new StockService(db);

        var product = new Product { Sku = "TST-004", Name = "Test Widget 4" };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Act
        var resultZero = await service.RecordMovementAsync(product.Id, StockMovementType.In, 0, "Invalid");
        var resultNegative = await service.RecordMovementAsync(product.Id, StockMovementType.In, -5, "Invalid");

        // Assert
        Assert.False(resultZero.Success);
        Assert.False(resultNegative.Success);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_IsRejected()
    {
        // Arrange
        using var db = CreateContext();
        var service = new StockService(db);

        await service.CreateProductAsync(new Product { Sku = "DUP-001", Name = "First" });

        // Act
        var result = await service.CreateProductAsync(new Product { Sku = "DUP-001", Name = "Second" });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_IsCaseInsensitive()
    {
        // Arrange
        using var db = CreateContext();
        var service = new StockService(db);

        await service.CreateProductAsync(new Product { Sku = "case-001", Name = "First" });

        // Act
        var result = await service.CreateProductAsync(new Product { Sku = "CASE-001", Name = "Second" });

        // Assert
        Assert.False(result.Success);
    }
}