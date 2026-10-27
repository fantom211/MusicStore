namespace MusicStore.Web.Helpers;

/// <summary>
/// Токен хранится в HttpOnly cookie (не в localStorage), поэтому недоступен JavaScript
/// и защищён от кражи через XSS. Токен из этой cookie подставляется в JWT-аутентификацию.
/// </summary>
public static class AuthCookie
{
    public const string Name = "MusicStore.Auth";

    public static CookieOptions CreateOptions(bool isHttps)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(12),
        };
    }
}
