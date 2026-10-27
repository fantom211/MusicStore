using System.ComponentModel.DataAnnotations;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;

namespace MusicStore.Web.MVC;

/// <summary>Модель страницы оформления заказа.</summary>
public class CheckoutViewModel
{
    public CartDto Cart { get; set; } = new();

    public DeliveryAddress Address { get; set; } = new();

    public bool UseSpinBonus { get; set; }

    /// <summary>Есть ли у пользователя активный бонус от «Слотов удачи».</summary>
    public bool HasActiveBonus { get; set; }

    public int ActiveBonusPercent { get; set; }

    public DateTime? ActiveBonusExpiresAt { get; set; }
}
