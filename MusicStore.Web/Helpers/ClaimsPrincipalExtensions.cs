using System.Security.Claims;

namespace MusicStore.Web.Helpers;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Идентификатор пользователя из JWT (claim sub / NameIdentifier).</summary>
    public static string GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user.FindFirstValue("sub");
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException("Пользователь не аутентифицирован или в токене нет идентификатора.");
        }

        return id;
    }
}
