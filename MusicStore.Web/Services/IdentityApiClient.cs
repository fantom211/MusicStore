using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;

namespace MusicStore.Web.Services;

/// <summary>Клиент IdentityService: регистрация, вход, данные текущего пользователя.</summary>
public class IdentityApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<AuthApiResult> RegisterAsync(AuthRegisterRequest request) => PostAsync("api/auth/register", request);

    public Task<AuthApiResult> LoginAsync(AuthLoginRequest request) => PostAsync("api/auth/login", request);

    /// <summary>Уведомить IdentityService о выходе. Токен всё равно удаляется на стороне сайта.</summary>
    public async Task LogoutAsync()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies[AuthCookie.Name];
        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            await _http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            // Сервис авторизации недоступен — выход всё равно выполняется локально.
        }
    }

    public async Task<UserProfileDto?> GetMeAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserProfileDto>(JsonOptions);
    }

    private async Task<AuthApiResult> PostAsync<TRequest>(string url, TRequest request)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(url, request, JsonOptions);
        }
        catch (HttpRequestException)
        {
            return new AuthApiResult
            {
                Success = false,
                Message = "Сервис авторизации недоступен. Убедитесь, что IdentityService запущен.",
            };
        }

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            return new AuthApiResult { Success = true, StatusCode = (int)response.StatusCode, Data = data };
        }

        var message = "Не удалось выполнить запрос к сервису авторизации.";
        var errors = Array.Empty<string>();
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
            if (error != null)
            {
                message = error.Message;
                errors = error.Errors;
            }
        }
        catch (JsonException)
        {
            // Ответ не распознан — оставляем общий текст ошибки.
        }

        return new AuthApiResult
        {
            Success = false,
            StatusCode = (int)response.StatusCode,
            Message = message,
            Errors = errors,
        };
    }
}
