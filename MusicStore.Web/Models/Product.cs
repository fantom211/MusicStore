using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public ProductCategory Category { get; set; }

    [Range(0.01, 10_000_000)]
    public decimal Price { get; set; }

    /// <summary>Процент скидки от 0 до 100. 0 — без скидки.</summary>
    [Range(0, 100)]
    public int DiscountPercent { get; set; }

    [Range(0, 1_000_000)]
    public int StockQuantity { get; set; }

    /// <summary>Путь к изображению, например /images/products/guitars.svg.</summary>
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
