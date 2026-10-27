using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.MVC;
using MusicStore.Web.Models;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers;

[Authorize]
public class SpinsController : Controller
{
    private readonly SpinService _spinService;

    public SpinsController(SpinService spinService)
    {
        _spinService = spinService;
    }

    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole(AppRoles.Admin);
        var status = await _spinService.GetStatusAsync(User.GetUserId(), isAdmin);
        return View(new SpinViewModel
        {
            CanSpin = status.CanSpin,
            TodayResult = status.TodayResult,
            ActiveBonus = status.ActiveBonus,
            IsAdminFunMode = isAdmin,
            AllSymbols = new[] { "💎", "🍒", "🔔", "⭐", "🍀", "🍋" },
        });
    }
}
