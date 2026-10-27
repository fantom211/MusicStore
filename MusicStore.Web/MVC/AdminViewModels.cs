using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;

namespace MusicStore.Web.MVC;

/// <summary>Модель списка товаров в админ-панели.</summary>
public class AdminIndexViewModel
{
    public PagedResult<ProductDto> Products { get; set; } = new();

    public string Search { get; set; } = string.Empty;

    public ProductCategory? Category { get; set; }
}
