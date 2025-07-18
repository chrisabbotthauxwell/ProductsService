using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Models;
using OrdersService.Services;
using OrdersService.Dtos;

namespace OrdersService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetAll()
        => Ok(_orderService.GetAll());

    [HttpGet("{id}")]
    public ActionResult<Order> GetById(string id)
    {
        var order = _orderService.GetById(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(
        [FromBody] OrderCreateDto dto,
        [FromServices] DaprClient daprClient)
    {
        var order = _orderService.Create(dto.ProductId, dto.Quantity);

        // Publish order-placed event
        var orderPlacedEvent = new
        {
            orderId = order.Id,
            productId = order.ProductId,
            quantity = order.Quantity
        };
        //await daprClient.PublishEventAsync("pubsub", "order-placed", orderPlacedEvent);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(string id, [FromBody] string status)
    {
        var updated = _orderService.UpdateStatus(id, status);
        return updated ? NoContent() : NotFound();
    }
}