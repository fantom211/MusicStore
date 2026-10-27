using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.MVC;

public class LoginViewModel
{
    [Required(ErrorMessage = "Укажите имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Укажите имя пользователя"), MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите email"), EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль"), MinLength(6, ErrorMessage = "Пароль должен быть не короче 6 символов")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
