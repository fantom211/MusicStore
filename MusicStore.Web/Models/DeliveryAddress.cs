using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.Models;

/// <summary>Адрес доставки (хранится как часть заказа).</summary>
public class DeliveryAddress
{
    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Street { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string House { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Apartment { get; set; }

    [Required, MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RecipientName { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }
}
