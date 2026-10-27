using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly CatalogService _catalogService;

    public ProductsApiController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>Список товаров: поиск, фильтры по категории и цене, только со скидкой, сортировка, пагинация.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductQuery query)
    {
        return Ok(await _catalogService.GetProductsAsync(query));
    }

    /// <summary>Один товар по идентификатору.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _catalogService.GetProductAsync(id);
        return product == null ? NotFound(new { message = "Товар не найден." }) : Ok(product);
    }
}

/// <summary>Администрирование товаров. Доступ только для роли Admin.</summary>
[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = AppRoles.Admin)]
public class AdminProductsApiController : ControllerBase
{
    private readonly CatalogService _catalogService;

    public AdminProductsApiController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>Все товары, включая неактивные.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductQuery query)
    {
        return Ok(await _catalogService.GetProductsAsync(query, includeInactive: true));
    }

    /// <summary>Создание товара.</summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsertRequest request)
    {
        var product = await _catalogService.CreateAsync(request);
        return CreatedAtAction(nameof(ProductsApiController.GetProduct), new { id = product.Id }, product);
    }

    /// <summary>Редактирование товара.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpsertRequest request)
    {
        return Ok(await _catalogService.UpdateAsync(id, request));
    }

    /// <summary>Удаление товара. Товар, встречающийся в заказах, удалить нельзя — только отключить.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _catalogService.DeleteAsync(id);
        return NoContent();
    }
}
