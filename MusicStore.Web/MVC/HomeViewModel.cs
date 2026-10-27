using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;

namespace MusicStore.Web.MVC;

/// <summary>Модель главной страницы: подборки товаров и категории со счётчиками.</summary>
public class HomeViewModel
{
    public IReadOnlyList<ProductDto> Popular { get; set; } = Array.Empty<ProductDto>();

    public IReadOnlyList<ProductDto> Discounted { get; set; } = Array.Empty<ProductDto>();

    public IReadOnlyList<CategoryRow> Categories { get; set; } = Array.Empty<CategoryRow>();

    public class CategoryRow
    {
        public ProductCategory Category { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
