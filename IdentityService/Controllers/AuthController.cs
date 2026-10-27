using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    /// <summary>Регистрация нового пользователя. Роль Admin таким способом назначить нельзя.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse { Message = "Проверьте правильность заполнения полей." });
        }

        var existingByName = await _userManager.FindByNameAsync(request.UserName);
        if (existingByName != null)
        {
            return Conflict(new ErrorResponse { Message = "Пользователь с таким именем уже существует." });
        }

        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail != null)
        {
            return Conflict(new ErrorResponse { Message = "Пользователь с таким email уже существует." });
        }

        var user = new ApplicationUser { UserName = request.UserName, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Не удалось создать пользователя.",
                Errors = result.Errors.Select(e => e.Description).ToArray(),
            });
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);

        return await CreateAuthResponse(user);
    }

    /// <summary>Вход по имени пользователя и паролю. Возвращает JWT.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse { Message = "Проверьте правильность заполнения полей." });
        }

        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new ErrorResponse { Message = "Неверное имя пользователя или пароль." });
        }

        return await CreateAuthResponse(user);
    }

    /// <summary>Данные текущего пользователя по его JWT.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new ErrorResponse { Message = "Пользователь не найден." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ToProfile(user, roles));
    }

    /// <summary>Выход. Сервис не хранит состояние сессий, клиент удаляет сохранённый токен.</summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Выход выполнен. Удалите токен на клиенте." });
    }

    private async Task<IActionResult> CreateAuthResponse(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.CreateToken(user, roles);
        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = ToProfile(user, roles),
        });
    }

    private static UserProfileDto ToProfile(ApplicationUser user, IList<string> roles)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles,
        };
    }
}
