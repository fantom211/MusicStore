using MusicStore.Web.Models;

namespace MusicStore.Web.Helpers;

/// <summary>Расчёт цен. Все денежные значения — decimal, расчёты выполняет сервер.</summary>
public static class PriceCalculator
{
    /// <summary>Цена товара после обычной скидки.</summary>
    public static decimal GetFinalPrice(decimal price, int discountPercent)
    {
        if (discountPercent <= 0)
        {
            return price;
        }

        return Math.Round(price * (1 - discountPercent / 100m), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Сумма после применения бонуса от «Слотов удачи» (процент применяется к сумме после обычных скидок).</summary>
    public static decimal ApplySpinDiscount(decimal itemsTotal, int spinDiscountPercent)
    {
        if (spinDiscountPercent <= 0)
        {
            return itemsTotal;
        }

        return Math.Round(itemsTotal * (1 - spinDiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
    }
}
