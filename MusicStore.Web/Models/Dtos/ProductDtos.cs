using System.ComponentModel.DataAnnotations;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;

namespace MusicStore.Web.Models.Dtos;

/// <summary>Товар в ответах API.</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }

    /// <summary>Процент скидки товара (0 — без скидки).</summary>
    public int DiscountPercent { get; set; }

    /// <summary>Цена после обычной скидки. Равна Price, если скидки нет.</summary>
    public decimal FinalPrice { get; set; }

    /// <summary>True, если у товара есть скидка (для показа старой и новой цены).</summary>
    public bool HasDiscount { get; set; }

    public int StockQuantity { get; set; }
    public bool InStock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public static ProductDto FromProduct(Product product)
    {
        var finalPrice = PriceCalculator.GetFinalPrice(product.Price, product.DiscountPercent);
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category.ToString(),
            Price = product.Price,
            DiscountPercent = product.DiscountPercent,
            FinalPrice = finalPrice,
            HasDiscount = product.DiscountPercent > 0,
            StockQuantity = product.StockQuantity,
            InStock = product.StockQuantity > 0,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
        };
    }
}

/// <summary>Страница результатов каталога.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}

/// <summary>Параметры каталога: поиск, фильтры, сортировка, пагинация.</summary>
public class ProductQuery
{
    /// <summary>Поиск по названию и описанию.</summary>
    public string? Search { get; set; }

    /// <summary>Фильтр по категории: Guitars, Strings, Pianos, Drums, Vinyl, Amplifiers.</summary>
    public ProductCategory? Category { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    /// <summary>true — только товары со скидкой.</summary>
    public bool? OnlyDiscounted { get; set; }

    /// <summary>Сортировка: name, price_asc, price_desc, discount, newest (по умолчанию newest).</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;

    public string GetSort() =>
        new[] { "name", "price_asc", "price_desc", "discount", "newest" }.Contains(Sort) ? Sort! : "newest";

    public int GetPage() => Math.Max(1, Page);

    public int GetPageSize() => Math.Clamp(PageSize, 1, 50);
}

/// <summary>Запрос на создание или редактирование товара (только Admin).</summary>
public class ProductUpsertRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ProductCategory Category { get; set; }

    [Range(0.01, 10_000_000)]
    public decimal Price { get; set; }

    [Range(0, 100)]
    public int DiscountPercent { get; set; }

    [Range(0, 1_000_000)]
    public int StockQuantity { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
