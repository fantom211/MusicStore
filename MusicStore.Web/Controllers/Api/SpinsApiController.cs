using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers.Api;

[ApiController]
[Route("api/spins")]
[Authorize]
public class SpinsApiController : ControllerBase
{
    private readonly SpinService _spinService;

    public SpinsApiController(SpinService spinService)
    {
        _spinService = spinService;
    }

    /// <summary>Статус «Слотов удачи»: можно ли крутить сегодня и активный бонус.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<SpinStatusDto>> GetStatus()
    {
        return Ok(await _spinService.GetStatusAsync(User.GetUserId(), User.IsInRole(AppRoles.Admin)));
    }

    /// <summary>Выполнить прокрутку. Результат (символы и скидка) определяется на сервере.
    /// Для Admin — фан-режим без ограничения по количеству, результат не сохраняется.</summary>
    [HttpPost("spin")]
    public async Task<ActionResult<SpinResultDto>> Spin()
    {
        return Ok(await _spinService.SpinAsync(User.GetUserId(), User.IsInRole(AppRoles.Admin)));
    }
}
