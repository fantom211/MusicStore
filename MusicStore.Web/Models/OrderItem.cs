using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.Models;

/// <summary>Позиция заказа. Цена фиксируется на момент оформления заказа.</summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }

    /// <summary>Название товара на момент заказа (снимок).</summary>
    [Required, MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Цена за единицу после обычной скидки товара, на момент заказа.</summary>
    public decimal UnitPrice { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    /// <summary>UnitPrice * Quantity.</summary>
    public decimal Subtotal { get; set; }
}
