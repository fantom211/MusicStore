using Microsoft.EntityFrameworkCore;
using MusicStore.Web.Data;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Services;

/// <summary>Корзина пользователя, хранится в БД.</summary>
public class CartService
{
    private readonly StoreDbContext _db;

    public CartService(StoreDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var items = await _db.CartItems
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Id)
            .ToListAsync();

        return ToCartDto(items);
    }

    public async Task<CartDto> AddToCartAsync(string userId, AddToCartRequest request)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId)
            ?? throw new ServiceException(404, "Товар не найден.");

        if (!product.IsActive)
        {
            throw new ServiceException(409, "Товар недоступен для покупки.");
        }

        if (product.StockQuantity <= 0)
        {
            throw new ServiceException(409, "Товара нет в наличии.");
        }

        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

        if (item == null)
        {
            item = new CartItem
            {
                UserId = userId,
                ProductId = product.Id,
                Quantity = Math.Min(request.Quantity, product.StockQuantity),
            };
            _db.CartItems.Add(item);
        }
        else
        {
            var newQuantity = item.Quantity + request.Quantity;
            if (newQuantity > product.StockQuantity)
            {
                throw new ServiceException(409, $"На складе только {product.StockQuantity} шт. товара «{product.Name}».");
            }

            item.Quantity = newQuantity;
        }

        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateQuantityAsync(string userId, int cartItemId, UpdateCartItemRequest request)
    {
        var item = await _db.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new ServiceException(404, "Позиция корзины не найдена.");

        if (request.Quantity > item.Product.StockQuantity)
        {
            throw new ServiceException(409, $"На складе только {item.Product.StockQuantity} шт. товара «{item.Product.Name}».");
        }

        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task RemoveItemAsync(string userId, int cartItemId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new ServiceException(404, "Позиция корзины не найдена.");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearAsync(string userId)
    {
        var items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    private static CartDto ToCartDto(List<CartItem> items)
    {
        var dtos = items.Select(c =>
        {
            var finalPrice = PriceCalculator.GetFinalPrice(c.Product.Price, c.Product.DiscountPercent);
            return new CartItemDto
            {
                CartItemId = c.Id,
                ProductId = c.Product.Id,
                ProductName = c.Product.Name,
                Category = c.Product.Category.ToString(),
                ImageUrl = c.Product.ImageUrl,
                Price = c.Product.Price,
                DiscountPercent = c.Product.DiscountPercent,
                FinalPrice = finalPrice,
                Quantity = c.Quantity,
                Subtotal = finalPrice * c.Quantity,
                StockQuantity = c.Product.StockQuantity,
            };
        }).ToList();

        return new CartDto
        {
            Items = dtos,
            Total = dtos.Sum(i => i.Subtotal),
        };
    }
}
