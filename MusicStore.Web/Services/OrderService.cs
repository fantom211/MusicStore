using Microsoft.EntityFrameworkCore;
using MusicStore.Web.Data;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Services;

/// <summary>
/// Оформление и просмотр заказов. Все цены считает сервер: цена позиций фиксируется
/// на момент заказа, скидка от «Слотов удачи» применяется к сумме после обычных скидок.
/// </summary>
public class OrderService
{
    private readonly StoreDbContext _db;

    public OrderService(StoreDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto> PlaceOrderAsync(string userId, PlaceOrderRequest request)
    {
        var cartItems = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Id)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            throw new ServiceException(400, "Корзина пуста — оформлять нечего.");
        }

        foreach (var item in cartItems)
        {
            if (!item.Product.IsActive)
            {
                throw new ServiceException(409, $"Товар «{item.Product.Name}» больше недоступен — удалите его из корзины.");
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                throw new ServiceException(409,
                    $"На складе недостаточно товара «{item.Product.Name}»: осталось {item.Product.StockQuantity} шт.");
            }
        }

        var itemsTotal = cartItems.Sum(c =>
            PriceCalculator.GetFinalPrice(c.Product.Price, c.Product.DiscountPercent) * c.Quantity);

        DailySpin? spin = null;
        var spinDiscountPercent = 0;
        if (request.UseSpinBonus)
        {
            // Загружаем как отслеживаемую сущность, чтобы пометить бонус использованным.
            spin = await _db.DailySpins
                .Where(s => s.UserId == userId && !s.IsUsed && s.DiscountPercent > 0 && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync()
                ?? throw new ServiceException(400,
                    "Активный бонус от «Слотов удачи» не найден: он отсутствует, уже использован или истёк.");

            spinDiscountPercent = spin.DiscountPercent;
        }

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.New,
            ItemsTotal = itemsTotal,
            SpinDiscountPercent = spinDiscountPercent,
            TotalAmount = PriceCalculator.ApplySpinDiscount(itemsTotal, spinDiscountPercent),
            Address = request.Address,
        };

        foreach (var cartItem in cartItems)
        {
            var unitPrice = PriceCalculator.GetFinalPrice(cartItem.Product.Price, cartItem.Product.DiscountPercent);
            order.Items.Add(new OrderItem
            {
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                Subtotal = unitPrice * cartItem.Quantity,
            });

            cartItem.Product.StockQuantity -= cartItem.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);

        if (spin != null)
        {
            // Бонус применяется максимум к одному заказу.
            spin.IsUsed = true;
            spin.UsedInOrder = order;
        }

        // Один SaveChanges выполняется в одной транзакции: заказ, списание склада,
        // очистка корзины и отметка бонуса либо применяются все вместе, либо никак.
        await _db.SaveChangesAsync();

        return ToOrderDto(order);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(ToOrderDto).ToList();
    }

    public async Task<OrderDto> GetUserOrderAsync(string userId, int orderId)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new ServiceException(404, "Заказ не найден.");

        return ToOrderDto(order);
    }

    /// <summary>Все заказы магазина (только Admin).</summary>
    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(ToOrderDto).ToList();
    }

    /// <summary>Смена статуса заказа (только Admin).</summary>
    public async Task<OrderDto> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new ServiceException(404, "Заказ не найден.");

        order.Status = status;
        await _db.SaveChangesAsync();
        return ToOrderDto(order);
    }

    public Task<DailySpin?> FindActiveBonusAsync(string userId)
    {
        return _db.DailySpins
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsUsed && s.DiscountPercent > 0 && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    private static OrderDto ToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            Status = order.Status.ToString(),
            UserId = order.UserId,
            ItemsTotal = order.ItemsTotal,
            SpinDiscountPercent = order.SpinDiscountPercent,
            TotalAmount = order.TotalAmount,
            Address = order.Address,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.Subtotal,
            }).ToList(),
        };
    }
}
