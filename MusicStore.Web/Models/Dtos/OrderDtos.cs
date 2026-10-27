using System.ComponentModel.DataAnnotations;
using MusicStore.Web.Models;

namespace MusicStore.Web.Models.Dtos;

/// <summary>Позиция заказа в ответах API.</summary>
public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>Заказ в ответах API.</summary>
public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    /// <summary>Сумма позиций после обычных скидок товаров.</summary>
    public decimal ItemsTotal { get; set; }

    /// <summary>Скидка от «Слотов удачи» (0, если не применялась).</summary>
    public int SpinDiscountPercent { get; set; }

    /// <summary>Итоговая стоимость заказа.</summary>
    public decimal TotalAmount { get; set; }

    public DeliveryAddress Address { get; set; } = new();
    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}

/// <summary>Оформление заказа из корзины.</summary>
public class PlaceOrderRequest
{
    [Required]
    public DeliveryAddress Address { get; set; } = new();

    /// <summary>Применить активный бонус от «Слотов удачи», если он есть.</summary>
    public bool UseSpinBonus { get; set; }
}

/// <summary>Обновление статуса заказа администратором.</summary>
public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; set; }
}
