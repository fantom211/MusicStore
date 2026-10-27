using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.Models;

public class Order
{
    public int Id { get; set; }

    /// <summary>Идентификатор пользователя из IdentityService.</summary>
    [Required, MaxLength(64)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.New;

    /// <summary>Сумма позиций после обычных скидок товаров.</summary>
    [Range(0, 100_000_000)]
    public decimal ItemsTotal { get; set; }

    /// <summary>Скидка от «Слотов удачи», применённая к заказу (0–20).</summary>
    public int SpinDiscountPercent { get; set; }

    /// <summary>Итоговая стоимость: ItemsTotal минус скидка от слотов.</summary>
    [Range(0, 100_000_000)]
    public decimal TotalAmount { get; set; }

    public DeliveryAddress Address { get; set; } = null!;

    public List<OrderItem> Items { get; set; } = new();
}
