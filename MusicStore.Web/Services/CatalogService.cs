using Microsoft.EntityFrameworkCore;
using MusicStore.Web.Data;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Services;

/// <summary>Каталог товаров: просмотр, фильтрация и администрирование.</summary>
public class CatalogService
{
    private readonly StoreDbContext _db;

    public CatalogService(StoreDbContext db)
    {
        _db = db;
    }

    /// <summary>Список товаров с поиском, фильтрами, сортировкой и пагинацией.</summary>
    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductQuery query, bool includeInactive = false)
    {
        var products = _db.Products.AsNoTracking();

        if (!includeInactive)
        {
            products = products.Where(p => p.IsActive);
        }

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            products = products.Where(p =>
                p.Name.ToLower().Contains(search.ToLower()) ||
                p.Description.ToLower().Contains(search.ToLower()));
        }

        if (query.Category.HasValue)
        {
            products = products.Where(p => p.Category == query.Category);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.Price >= query.MinPrice);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= query.MaxPrice);
        }

        if (query.OnlyDiscounted == true)
        {
            products = products.Where(p => p.DiscountPercent > 0);
        }

        products = query.GetSort() switch
        {
            "name" => products.OrderBy(p => p.Name),
            "price_asc" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            "discount" => products.OrderByDescending(p => p.DiscountPercent).ThenBy(p => p.Name),
            _ => products.OrderByDescending(p => p.CreatedAt),
        };

        var page = query.GetPage();
        var pageSize = query.GetPageSize();
        var totalCount = await products.CountAsync();
        var items = await products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = items.Select(ProductDto.FromProduct).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    /// <summary>Количество активных товаров по категориям (для главной страницы).</summary>
    public async Task<IReadOnlyDictionary<ProductCategory, int>> GetActiveCountsByCategoryAsync()
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .GroupBy(p => p.Category)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    /// <summary>Один товар. Возвращает null, если товар не найден или скрыт.</summary>
    public async Task<ProductDto?> GetProductAsync(int id, bool includeInactive = false)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null || (!includeInactive && !product.IsActive))
        {
            return null;
        }

        return ProductDto.FromProduct(product);
    }

    /// <summary>Создание товара (только Admin).</summary>
    public async Task<ProductDto> CreateAsync(ProductUpsertRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            DiscountPercent = request.DiscountPercent,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return ProductDto.FromProduct(product);
    }

    /// <summary>Редактирование товара (только Admin).</summary>
    public async Task<ProductDto> UpdateAsync(int id, ProductUpsertRequest request)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new ServiceException(404, "Товар не найден.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.Price = request.Price;
        product.DiscountPercent = request.DiscountPercent;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;

        await _db.SaveChangesAsync();
        return ProductDto.FromProduct(product);
    }

    /// <summary>Удаление товара (только Admin). Товар из заказов удалить нельзя — только отключить.</summary>
    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new ServiceException(404, "Товар не найден.");

        var usedInOrders = await _db.OrderItems.AnyAsync(i => i.ProductId == id);
        if (usedInOrders)
        {
            throw new ServiceException(409,
                "Товар присутствует в заказах, поэтому его нельзя удалить. Отключите его (IsActive = false).");
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }
}
