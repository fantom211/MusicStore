using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers;

public class CatalogController : Controller
{
    private readonly CatalogService _catalogService;

    public CatalogController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(
        string? search, ProductCategory? category, decimal? minPrice, decimal? maxPrice,
        bool onlyDiscounted, string? sort, int page = 1)
    {
        var query = new ProductQuery
        {
            Search = search,
            Category = category,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            OnlyDiscounted = onlyDiscounted ? true : null,
            Sort = sort,
            Page = page,
            PageSize = 12,
        };

        var products = await _catalogService.GetProductsAsync(query);

        return View(new MVC.CatalogIndexViewModel
        {
            Products = products,
            Search = search ?? string.Empty,
            Category = category?.ToString(),
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            OnlyDiscounted = onlyDiscounted,
            Sort = query.GetSort(),
        });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _catalogService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
}
