using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.Models.Dtos;

/// <summary>Позиция корзины в ответах API.</summary>
public class CartItemDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }
    public int Quantity { get; set; }

    /// <summary>FinalPrice * Quantity.</summary>
    public decimal Subtotal { get; set; }

    public int StockQuantity { get; set; }
}

/// <summary>Корзина пользователя в ответах API.</summary>
public class CartDto
{
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();

    /// <summary>Сумма всех позиций (цены уже с обычными скидками товаров).</summary>
    public decimal Total { get; set; }

    public int TotalCount => Items.Sum(i => i.Quantity);
}

public class AddToCartRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(1, 1000)]
    public int Quantity { get; set; }
}
