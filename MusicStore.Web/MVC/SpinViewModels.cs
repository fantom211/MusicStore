using MusicStore.Web.Models.Dtos;

namespace MusicStore.Web.MVC;

/// <summary>Модель страницы «Слоты удачи».</summary>
public class SpinViewModel
{
    public bool CanSpin { get; set; }

    /// <summary>Администратор: фан-режим, крутить можно без ограничений (без сохранения бонуса).</summary>
    public bool IsAdminFunMode { get; set; }

    public SpinResultDto? TodayResult { get; set; }

    public SpinResultDto? ActiveBonus { get; set; }

    /// <summary>Все символы барабанов — для анимации на клиенте.</summary>
    public IReadOnlyList<string> AllSymbols { get; set; } = Array.Empty<string>();
}
