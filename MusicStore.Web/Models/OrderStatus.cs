namespace MusicStore.Web.Models;

/// <summary>Статусы заказа.</summary>
public enum OrderStatus
{
    New = 1,        // новый
    Paid = 2,       // оплачен
    Shipped = 3,    // отправлен
    Delivered = 4,  // доставлен
    Cancelled = 5,  // отменён
}
