using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.MVC;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers;

public class HomeController : Controller
{
    private readonly CatalogService _catalogService;

    public HomeController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var popular = await _catalogService.GetProductsAsync(new ProductQuery
        {
            Sort = "newest",
            PageSize = 4,
        });

        var discounted = await _catalogService.GetProductsAsync(new ProductQuery
        {
            OnlyDiscounted = true,
            Sort = "discount",
            PageSize = 4,
        });

        var counts = await _catalogService.GetActiveCountsByCategoryAsync();
        var categories = Enum.GetValues<ProductCategory>()
            .Select(c => new HomeViewModel.CategoryRow
            {
                Category = c,
                Name = c.GetDisplayName(),
                Count = counts.GetValueOrDefault(c),
            })
            .ToList();

        return View(new HomeViewModel
        {
            Popular = popular.Items,
            Discounted = discounted.Items,
            Categories = categories,
        });
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
