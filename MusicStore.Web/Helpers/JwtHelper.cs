using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MusicStore.Web.Helpers;

/// <summary>Ключ проверки JWT — тот же, которым подписывает IdentityService.</summary>
public static class JwtHelper
{
    public static SymmetricSecurityKey GetSigningKey(IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Не задан Jwt:Key. Установите его через user-secrets: dotnet user-secrets set \"Jwt:Key\" \"<ключ>\" " +
                "— ключ должен совпадать с ключом IdentityService.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}
