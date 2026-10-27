using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.MVC;
using MusicStore.Web.Models;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers;

public class AccountController : Controller
{
    private readonly IdentityApiClient _identityApiClient;
    private readonly OrderService _orderService;

    public AccountController(IdentityApiClient identityApiClient, OrderService orderService)
    {
        _identityApiClient = identityApiClient;
        _orderService = orderService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _identityApiClient.LoginAsync(new AuthLoginRequest
        {
            UserName = model.UserName,
            Password = model.Password,
        });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        SaveTokenInCookie(result.Data.Token);
        return RedirectToAction("Index", "Catalog");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _identityApiClient.RegisterAsync(new AuthRegisterRequest
        {
            UserName = model.UserName,
            Email = model.Email,
            Password = model.Password,
        });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        SaveTokenInCookie(result.Data.Token);
        return RedirectToAction("Index", "Catalog");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Сервис не хранит сессии — просто уведомляем и удаляем cookie с токеном.
        await _identityApiClient.LogoutAsync();
        Response.Cookies.Delete(AuthCookie.Name);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Личный кабинет: профиль, активный бонус, последние заказы.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = User.GetUserId();
        var token = Request.Cookies[AuthCookie.Name] ?? string.Empty;

        var profile = await _identityApiClient.GetMeAsync(token);
        var orders = await _orderService.GetUserOrdersAsync(userId);
        var bonus = await _orderService.FindActiveBonusAsync(userId);

        return View(new ProfileViewModel
        {
            Profile = profile,
            UserName = User.Identity?.Name ?? "музыкант",
            RecentOrders = orders.Take(5).ToList(),
            ActiveBonus = bonus == null
                ? null
                : new Models.Dtos.SpinResultDto
                {
                    DiscountPercent = bonus.DiscountPercent,
                    ExpiresAt = bonus.ExpiresAt,
                    IsUsed = bonus.IsUsed,
                },
        });
    }

    private void SaveTokenInCookie(string token)
    {
        Response.Cookies.Append(AuthCookie.Name, token, AuthCookie.CreateOptions(Request.IsHttps));
    }
}
