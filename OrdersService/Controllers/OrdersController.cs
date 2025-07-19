using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrdersService.Models;
using OrdersService.Services;
using OrdersService.Dtos;

namespace OrdersService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetAll()
    {
        _logger.LogInformation("Getting all orders");
        return Ok(_orderService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Order> GetById(string id)
    {
        _logger.LogInformation("Getting order by id: {OrderId}", id);
        var order = _orderService.GetById(id);
        if (order is null)
        {
            _logger.LogWarning("Order not found: {OrderId}", id);
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(
        [FromBody] OrderCreateDto dto,
        [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("Creating new order for product {ProductId} with quantity {Quantity}", dto.ProductId, dto.Quantity);
        var order = _orderService.Create(dto.ProductId, dto.Quantity);

        // Publish order-placed event
        var orderPlacedEvent = new
        {
            orderId = order.Id,
            productId = order.ProductId,
            quantity = order.Quantity
        };
        await daprClient.PublishEventAsync("pubsub", "order-placed", orderPlacedEvent);
        _logger.LogInformation("Published order-placed event for order {OrderId}", order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(string id, [FromBody] string status)
    {
        _logger.LogInformation("Updating status for order {OrderId} to {Status}", id, status);
        var updated = _orderService.UpdateStatus(id, status);
        if (!updated)
        {
            _logger.LogWarning("Order not found for status update: {OrderId}", id);
            return NotFound();
        }
        return NoContent();
    }
}