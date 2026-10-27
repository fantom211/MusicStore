using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync(User.GetUserId());
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        try
        {
            await _cartService.AddToCartAsync(User.GetUserId(), new AddToCartRequest
            {
                ProductId = productId,
                Quantity = Math.Max(1, quantity),
            });
            TempData["Success"] = "Товар добавлен в корзину.";
        }
        catch (Services.ServiceException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int cartItemId, int quantity)
    {
        try
        {
            await _cartService.UpdateQuantityAsync(User.GetUserId(), cartItemId, new UpdateCartItemRequest
            {
                Quantity = Math.Max(1, quantity),
            });
        }
        catch (Services.ServiceException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        try
        {
            await _cartService.RemoveItemAsync(User.GetUserId(), cartItemId);
        }
        catch (Services.ServiceException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
