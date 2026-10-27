using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers.Api;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartApiController : ControllerBase
{
    private readonly CartService _cartService;

    public CartApiController(CartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>Корзина текущего пользователя.</summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        return Ok(await _cartService.GetCartAsync(User.GetUserId()));
    }

    /// <summary>Добавить товар в корзину.</summary>
    [HttpPost]
    public async Task<ActionResult<CartDto>> Add(AddToCartRequest request)
    {
        return Ok(await _cartService.AddToCartAsync(User.GetUserId(), request));
    }

    /// <summary>Изменить количество позиции.</summary>
    [HttpPut("{cartItemId:int}")]
    public async Task<ActionResult<CartDto>> Update(int cartItemId, UpdateCartItemRequest request)
    {
        return Ok(await _cartService.UpdateQuantityAsync(User.GetUserId(), cartItemId, request));
    }

    /// <summary>Удалить позицию из корзины.</summary>
    [HttpDelete("{cartItemId:int}")]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        await _cartService.RemoveItemAsync(User.GetUserId(), cartItemId);
        return NoContent();
    }

    /// <summary>Очистить корзину.</summary>
    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        await _cartService.ClearAsync(User.GetUserId());
        return NoContent();
    }
}
