namespace MusicStore.Web.Models.Dtos;

/// <summary>Статус «Слотов удачи» на сегодня.</summary>
public class SpinStatusDto
{
    /// <summary>Можно ли прокрутить прямо сейчас (одна попытка в сутки).</summary>
    public bool CanSpin { get; set; }

    /// <summary>Дата, за которую уже есть прокрутка (UTC), null если сегодня не крутили.</summary>
    public DateOnly? LastSpinDate { get; set; }

    /// <summary>Результат сегодняшней прокрутки, если она была. Символы барабанов видны только после прокрутки.</summary>
    public SpinResultDto? TodayResult { get; set; }

    /// <summary>Активный неиспользованный бонус, который можно применить к заказу.</summary>
    public SpinResultDto? ActiveBonus { get; set; }
}

/// <summary>Результат прокрутки (или активный бонус).</summary>
public class SpinResultDto
{
    /// <summary>Выпавшая скидка: 0, 5, 10, 15 или 20 процентов.</summary>
    public int DiscountPercent { get; set; }

    /// <summary>Символы барабанов, выпавшие на сервере. Клиент только анимирует их.</summary>
    public IReadOnlyList<string> Symbols { get; set; } = Array.Empty<string>();

    /// <summary>Дата прокрутки (UTC).</summary>
    public DateOnly SpinDate { get; set; }

    /// <summary>Когда заканчивается действие скидки. Для 0% — null.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>True, если скидка уже применена к заказу.</summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Фан-режим для администратора: прокрутка не сохраняется в БД и не даёт бонус,
    /// крутить можно без ограничений.
    /// </summary>
    public bool IsDemo { get; set; }
}
