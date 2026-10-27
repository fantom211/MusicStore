namespace MusicStore.Web.Services;

/// <summary>
/// Бизнес-ошибка сервиса с HTTP-статусом. Контроллеры не обрабатывают её вручную —
/// ApiExceptionFilter превращает её в понятный ответ API.
/// </summary>
public class ServiceException : Exception
{
    public int StatusCode { get; }

    public ServiceException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
