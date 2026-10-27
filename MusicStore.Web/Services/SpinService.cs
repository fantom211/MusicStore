using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using MusicStore.Web.Data;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Services;

/// <summary>
/// «Слоты удачи» — бесплатная игровая механика: одна попытка в сутки, результат
/// определяется только на сервере. Запрет повторной прокрутки обеспечивается
/// уникальным индексом (UserId, SpinDate) в базе данных.
/// </summary>
public class SpinService
{
    private const string Diamond = "💎";
    private static readonly string[] RegularSymbols = { "🍒", "🔔", "⭐", "🍀", "🍋" };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly StoreDbContext _db;

    public SpinService(StoreDbContext db)
    {
        _db = db;
    }

    public async Task<SpinStatusDto> GetStatusAsync(string userId, bool isAdmin = false)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todaySpin = await _db.DailySpins
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SpinDate == today);

        var activeBonus = await FindActiveBonusAsync(userId);

        return new SpinStatusDto
        {
            // Администратор крутит в фан-режиме без ограничения «раз в сутки».
            CanSpin = isAdmin || todaySpin == null,
            LastSpinDate = todaySpin?.SpinDate,
            TodayResult = todaySpin == null ? null : ToResult(todaySpin),
            ActiveBonus = activeBonus == null ? null : ToResult(activeBonus),
        };
    }

    /// <summary>
    /// Выполнить прокрутку. Распределение: 45% — без скидки, 30% — 5%, 15% — 10%, 8% — 15%, 2% — 20%.
    /// Для администратора включён фан-режим: результат не пишется в БД и бонус не выдаётся.
    /// </summary>
    public Task<SpinResultDto> SpinAsync(string userId, bool isAdmin = false)
    {
        if (isAdmin)
        {
            var demo = new SpinResultDto
            {
                DiscountPercent = DrawDiscountPercent(),
                SpinDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsDemo = true,
            };
            demo.Symbols = DrawSymbols(demo.DiscountPercent);
            return Task.FromResult(demo);
        }

        return SpinRegularAsync(userId);
    }

    private async Task<SpinResultDto> SpinRegularAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var existing = await _db.DailySpins
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SpinDate == today);
        if (existing != null)
        {
            throw new ServiceException(409, "Сегодня вы уже прокрутили «Слоты удачи». Возвращайтесь завтра!");
        }

        var discountPercent = DrawDiscountPercent();
        var spin = new DailySpin
        {
            UserId = userId,
            SpinDate = today,
            DiscountPercent = discountPercent,
            SymbolsJson = JsonSerializer.Serialize(DrawSymbols(discountPercent), JsonOptions),
            ExpiresAt = discountPercent > 0 ? DateTime.UtcNow.AddHours(24) : null,
        };

        _db.DailySpins.Add(spin);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Гонка параллельных запросов: уникальный индекс (UserId, SpinDate) сработал.
            throw new ServiceException(409, "Сегодня вы уже прокрутили «Слоты удачи». Возвращайтесь завтра!");
        }

        return ToResult(spin);
    }

    public Task<DailySpin?> FindActiveBonusAsync(string userId)
    {
        return _db.DailySpins
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsUsed && s.DiscountPercent > 0 && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    private static int DrawDiscountPercent()
    {
        var roll = Random.Shared.NextDouble();
        return roll switch
        {
            < 0.45 => 0,   // без скидки — 45%
            < 0.75 => 5,   // 30%
            < 0.90 => 10,  // 15%
            < 0.98 => 15,  // 8%
            _ => 20,       // 2%
        };
    }

    /// <summary>Символы барабанов подбираются под выпавший процент.</summary>
    private static string[] DrawSymbols(int percent)
    {
        return percent switch
        {
            // Три алмаза — 20%.
            20 => new[] { Diamond, Diamond, Diamond },
            // Три одинаковых обычных символа — 15%.
            15 => Enumerable.Repeat(RegularSymbols[Random.Shared.Next(RegularSymbols.Length)], 3).ToArray(),
            // Два алмаза — 10%.
            10 => Shuffle(new[] { Diamond, Diamond, RegularSymbols[Random.Shared.Next(RegularSymbols.Length)] }),
            // Пара обычных символов — 5%.
            5 => Shuffle(new[] { PickRegular(), PickRegularExcept(PickRegular()) }),
            // Остальные сочетания — без скидки: три разных обычных символа.
            _ => Shuffle(RegularSymbols.OrderBy(_ => Random.Shared.Next()).Take(3).ToArray()),
        };
    }

    private static string PickRegular() => RegularSymbols[Random.Shared.Next(RegularSymbols.Length)];

    private static string PickRegularExcept(string exclude)
    {
        string symbol;
        do
        {
            symbol = PickRegular();
        }
        while (symbol == exclude);

        return symbol;
    }

    private static string[] Shuffle(string[] symbols) =>
        symbols.OrderBy(_ => Random.Shared.Next()).ToArray();

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is NpgsqlException { SqlState: "23505" };

    private static SpinResultDto ToResult(DailySpin spin)
    {
        return new SpinResultDto
        {
            DiscountPercent = spin.DiscountPercent,
            Symbols = JsonSerializer.Deserialize<string[]>(spin.SymbolsJson, JsonOptions) ?? Array.Empty<string>(),
            SpinDate = spin.SpinDate,
            ExpiresAt = spin.ExpiresAt,
            IsUsed = spin.IsUsed,
        };
    }
}
