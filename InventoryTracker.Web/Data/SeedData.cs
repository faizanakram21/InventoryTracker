using InventoryTracker.Web.Models;

namespace InventoryTracker.Web.Data;

public static class SeedData
{
    public static void Seed(InventoryDbContext db)
    {
        if (db.Products.Any()) return;

        var p1 = new Product { Sku = "SKU-001", Name = "Wireless Mouse", ReorderLevel = 10 };
        var p2 = new Product { Sku = "SKU-002", Name = "Mechanical Keyboard", ReorderLevel = 5 };
        var p3 = new Product { Sku = "SKU-003", Name = "USB-C Cable", ReorderLevel = 20 };

        db.Products.AddRange(p1, p2, p3);
        db.SaveChanges();

        db.StockMovements.AddRange(
            new StockMovement { ProductId = p1.Id, Type = StockMovementType.In, Quantity = 50, Note = "Initial stock" },
            new StockMovement { ProductId = p1.Id, Type = StockMovementType.Out, Quantity = 45, Note = "Bulk order" },
            new StockMovement { ProductId = p2.Id, Type = StockMovementType.In, Quantity = 8, Note = "Initial stock" },
            new StockMovement { ProductId = p3.Id, Type = StockMovementType.In, Quantity = 15, Note = "Initial stock" }
        );
        db.SaveChanges();
    }
}