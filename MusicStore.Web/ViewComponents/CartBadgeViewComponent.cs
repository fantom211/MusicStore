using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Services;
using System.Security.Claims;

namespace MusicStore.Web.ViewComponents;

/// <summary>Счётчик товаров в корзине для шапки сайта. Для гостей — 0 (значок скрыт).</summary>
public class CartBadgeViewComponent : ViewComponent
{
    private readonly CartService _cartService;

    public CartBadgeViewComponent(CartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = 0;
        if (User is ClaimsPrincipal principal && principal.Identity?.IsAuthenticated == true)
        {
            try
            {
                count = (await _cartService.GetCartAsync(principal.GetUserId())).TotalCount;
            }
            catch
            {
                // Шапка не должна падать из-за сбоя корзины.
            }
        }

        return View(count);
    }
}
