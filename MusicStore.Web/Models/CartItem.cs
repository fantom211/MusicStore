using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.Models;

/// <summary>Позиция корзины пользователя. Корзина хранится в БД и привязана к пользователю.</summary>
public class CartItem
{
    public int Id { get; set; }

    /// <summary>Идентификатор пользователя из IdentityService.</summary>
    [Required, MaxLength(64)]
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}
