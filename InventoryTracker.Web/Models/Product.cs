using System.ComponentModel.DataAnnotations;

namespace InventoryTracker.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
    public int ReorderLevel { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public List<StockMovement> Movements { get; set; } = new();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int CurrentStock => Movements
        .Sum(m => m.Type == StockMovementType.In ? m.Quantity : -m.Quantity);
}