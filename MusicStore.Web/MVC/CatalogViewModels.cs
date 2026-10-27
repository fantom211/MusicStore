using MusicStore.Web.Models.Dtos;

namespace MusicStore.Web.MVC;

/// <summary>Модель страницы каталога: страница результатов + параметры фильтрации.</summary>
public class CatalogIndexViewModel
{
    public PagedResult<ProductDto> Products { get; set; } = new();

    public string Search { get; set; } = string.Empty;

    public string? Category { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool OnlyDiscounted { get; set; }

    public string Sort { get; set; } = "newest";
}
