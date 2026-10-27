using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Web.Helpers;
using MusicStore.Web.Models;
using MusicStore.Web.Models.Dtos;
using MusicStore.Web.Services;

namespace MusicStore.Web.Controllers.Api;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersApiController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersApiController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Оформить заказ из корзины с адресом доставки и (по желанию) бонусом от слотов.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Checkout(PlaceOrderRequest request)
    {
        var order = await _orderService.PlaceOrderAsync(User.GetUserId(), request);
        return Ok(order);
    }

    /// <summary>Заказы текущего пользователя.</summary>
    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        return Ok(await _orderService.GetUserOrdersAsync(User.GetUserId()));
    }

    /// <summary>Все заказы магазина (только Admin).</summary>
    [HttpGet("all")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
    {
        return Ok(await _orderService.GetAllOrdersAsync());
    }

    /// <summary>Один заказ текущего пользователя.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        return Ok(await _orderService.GetUserOrderAsync(User.GetUserId(), id));
    }

    /// <summary>Смена статуса заказа (только Admin).</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        return Ok(await _orderService.UpdateStatusAsync(id, request.Status));
    }
}
