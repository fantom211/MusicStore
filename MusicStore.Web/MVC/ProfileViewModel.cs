using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;

namespace MusicStore.Web.MVC;

/// <summary>Модель личного кабинета: профиль, активный бонус, последние заказы.</summary>
public class ProfileViewModel
{
    /// <summary>Данные из IdentityService; null, если сервис недоступен.</summary>
    public UserProfileDto? Profile { get; set; }

    public string UserName { get; set; } = string.Empty;

    public IReadOnlyList<OrderDto> RecentOrders { get; set; } = Array.Empty<OrderDto>();

    public SpinResultDto? ActiveBonus { get; set; }
}
