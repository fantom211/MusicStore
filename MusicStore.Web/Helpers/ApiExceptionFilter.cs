using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MusicStore.Web.Services;

namespace MusicStore.Web.Helpers;

/// <summary>Превращает ServiceException в ответ API с кодом и сообщением, остальные исключения — в 500.</summary>
public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ServiceException serviceException)
        {
            context.Result = new ObjectResult(new { message = serviceException.Message })
            {
                StatusCode = serviceException.StatusCode,
            };
            context.ExceptionHandled = true;
            return;
        }

        if (!context.ExceptionHandled)
        {
            context.Result = new ObjectResult(new { message = "Внутренняя ошибка сервера." })
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
            context.ExceptionHandled = true;
        }
    }
}
