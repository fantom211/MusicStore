using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.MVC;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly OrderService _orderService;
    private readonly CartService _cartService;

    public OrdersController(OrderService orderService, CartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _orderService.GetUserOrdersAsync(User.GetUserId()));
    }

    public async Task<IActionResult> Details(int id)
    {
        return View(await _orderService.GetUserOrderAsync(User.GetUserId(), id));
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var userId = User.GetUserId();
        var cart = await _cartService.GetCartAsync(userId);
        if (cart.Items.Count == 0)
        {
            TempData["Error"] = "Корзина пуста — сначала добавьте товары.";
            return RedirectToAction("Index", "Cart");
        }

        return View(await BuildCheckoutModel(userId, cart, new CheckoutViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var userId = User.GetUserId();
        var cart = await _cartService.GetCartAsync(userId);

        if (!ModelState.IsValid)
        {
            return View(await BuildCheckoutModel(userId, cart, model));
        }

        try
        {
            var order = await _orderService.PlaceOrderAsync(userId, new PlaceOrderRequest
            {
                Address = model.Address,
                UseSpinBonus = model.UseSpinBonus,
            });
            TempData["Success"] = $"Заказ №{order.Id} оформлен!";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
        catch (ServiceException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildCheckoutModel(userId, cart, model));
        }
    }

    private async Task<CheckoutViewModel> BuildCheckoutModel(string userId, CartDto cart, CheckoutViewModel model)
    {
        var bonus = await _orderService.FindActiveBonusAsync(userId);
        model.Cart = cart;
        model.HasActiveBonus = bonus != null;
        model.ActiveBonusPercent = bonus?.DiscountPercent ?? 0;
        model.ActiveBonusExpiresAt = bonus?.ExpiresAt;
        return model;
    }
}
