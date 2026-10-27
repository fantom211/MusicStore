using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.Models;

/// <summary>Результат ежедневной прокрутки «Слотов удачи».</summary>
public class DailySpin
{
    public int Id { get; set; }

    /// <summary>Идентификатор пользователя из IdentityService.</summary>
    [Required, MaxLength(64)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Дата прокрутки (UTC). Уникальный индекс (UserId, SpinDate) не даёт прокрутить дважды за день.</summary>
    public DateOnly SpinDate { get; set; }

    /// <summary>Выпавшая скидка: 0, 5, 10, 15 или 20 процентов.</summary>
    [Range(0, 20)]
    public int DiscountPercent { get; set; }

    /// <summary>Символы барабанов, выпавшие сервером, например ["💎","💎","🍒"].</summary>
    [Required, MaxLength(100)]
    public string SymbolsJson { get; set; } = string.Empty;

    /// <summary>Когда заканчивается действие выигранной скидки (через 24 часа). Для 0% — null.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Скидка уже применена к заказу.</summary>
    public bool IsUsed { get; set; }

    /// <summary>Заказ, в котором применена скидка.</summary>
    public int? UsedInOrderId { get; set; }

    public Order? UsedInOrder { get; set; }
}
