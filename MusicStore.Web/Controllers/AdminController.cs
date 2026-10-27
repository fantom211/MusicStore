using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.MVC;
using MusicStore.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MusicStore.Web.Controllers;

/// <summary>Админ-панель: управление товарами через CatalogService. Только роль Admin.</summary>
[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private readonly CatalogService _catalogService;
    private readonly IWebHostEnvironment _environment;
    public AdminController(
    CatalogService catalogService,
    IWebHostEnvironment environment)
    {
        _catalogService = catalogService;
        _environment = environment;
    }

    public async Task<IActionResult> Index(string? search, ProductCategory? category, int page = 1)
    {
        var query = new ProductQuery
        {
            Search = search,
            Category = category,
            Sort = "name",
            Page = page,
            PageSize = 15,
        };

        var products = await _catalogService.GetProductsAsync(query, includeInactive: true);

        return View(new AdminIndexViewModel
        {
            Products = products,
            Search = search ?? string.Empty,
            Category = category,
        });
    }

    [HttpGet]
    public IActionResult Create() => View(new ProductUpsertRequest());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductUpsertRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }
        if (request.ImageFile is { Length: > 0 })
        {
            request.ImageUrl = await SaveImageAsync(request.ImageFile);
        }
        try
        {
            await _catalogService.CreateAsync(request);
            TempData["Success"] = "Товар создан.";
            return RedirectToAction(nameof(Index));
        }
        catch (ServiceException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _catalogService.GetProductAsync(id, includeInactive: true);
        if (product == null)
        {
            return NotFound();
        }

        return View(new ProductUpsertRequest
        {
            Name = product.Name,
            Description = product.Description,
            Category = Enum.Parse<ProductCategory>(product.Category),
            Price = product.Price,
            DiscountPercent = product.DiscountPercent,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductUpsertRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            if (request.ImageFile is { Length: > 0 })
            {
                request.ImageUrl = await SaveImageAsync(request.ImageFile);
            }

            await _catalogService.UpdateAsync(id, request);

            TempData["Success"] = "Изменения сохранены.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(request.ImageFile), ex.Message);
            return View(request);
        }
        catch (ServiceException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _catalogService.DeleteAsync(id);
            TempData["Success"] = "Товар удалён.";
        }
        catch (ServiceException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        const long maxFileSize = 5 * 1024 * 1024;

        if (file.Length == 0 || file.Length > maxFileSize)
            throw new InvalidOperationException(
                "Размер изображения должен быть от 1 байта до 5 МБ.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException(
                "Разрешены только JPG, JPEG, PNG и WEBP.");

        var folder = Path.Combine(
            _environment.WebRootPath, "images", "products");

        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(
            fullPath, FileMode.CreateNew, FileAccess.Write);

        await file.CopyToAsync(stream);

        return $"/images/products/{fileName}";
    }
}
