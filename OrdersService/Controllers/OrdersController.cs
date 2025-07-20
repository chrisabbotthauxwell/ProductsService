using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Models;
using OrdersService.Services;
using OrdersService.Dtos;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Creating order for product {ProductId} with quantity {Quantity}", dto.ProductId, dto.Quantity);
        try
        {
            var order = _orderService.Create(dto.ProductId, dto.Quantity);
            var orderPlacedEvent = new OrderPlacedEventDto
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity
            };
            await daprClient.PublishEventAsync("pubsub", "order-placed", orderPlacedEvent);
            _logger.LogInformation("Published order-placed event for order {OrderId}", order.Id);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for product {ProductId}", dto.ProductId);
            throw;
        }
    }

    // Dapr subscription for stock-available topic
    [HttpPost("/dapr/subscribe/stock-available")]
    [Topic("pubsub", "stock-available")]
    public async Task<IActionResult> OnStockAvailable(
        [FromBody] StockAvailableEventDto stockAvailable,
        [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("Received stock-available event for product {ProductId} with stock {StockCount}", stockAvailable.ProductId, stockAvailable.StockCount);

        // Find all pending orders for this product, oldest first
        var pendingOrders = _orderService.GetAll()
            .Where(o => o.ProductId == stockAvailable.ProductId && o.Status == "pending")
            .OrderBy(o => o.UpdatedAt)
            .ToList();

        int availableStock = stockAvailable.StockCount;

        foreach (var order in pendingOrders)
        {
            if (order.Quantity <= availableStock)
            {
                // Fulfil the order
                _orderService.UpdateStatus(order.Id, "fulfilled");
                availableStock -= order.Quantity;

                // Publish stock-updated event
                var stockUpdatedEvent = new StockUpdatedEventDto
                {
                    OrderId = order.Id,
                    ProductId = order.ProductId,
                    Quantity = order.Quantity
                };
                await daprClient.PublishEventAsync("pubsub", "stock-updated", stockUpdatedEvent);

                _logger.LogInformation("Order {OrderId} fulfilled and stock-updated event published", order.Id);

                // Stop after fulfilling one order as per requirements
                // break;
            }
            else
            {
                // Not enough stock to fulfil this order, skip it
                _logger.LogInformation("Not enough stock to fulfil order {OrderId}, skipping", order.Id);

                // Update order updatedAt timestamp
                _orderService.UpdateUpdatedAt(order.Id, DateTime.UtcNow);
            }
        }

        return Ok();
    }

    // Dapr subscription for order-fulfilled topic
    [HttpPost("/dapr/subscribe/order-fulfilled")]  
    [Topic("pubsub", "order-fulfilled")]
    public async Task<IActionResult> OnOrderFulfilled(
        [FromBody] OrderFulfilledEventDto orderFulfilled,
        [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("Received order-fulfilled event for order {OrderId}, product {ProductId}, quantity {Quantity}", orderFulfilled.OrderId, orderFulfilled.ProductId, orderFulfilled.Quantity);

        // Logic to handle fulfilled orders can be added here if needed
        var order = _orderService.GetById(orderFulfilled.OrderId);
        if (order is null)
        {
            _logger.LogWarning("Order not found for fulfillment: {OrderId}", orderFulfilled.OrderId);
            return NotFound();
        }
        else
        {
            _orderService.UpdateStatus(order.Id, "fulfilled");
            _logger.LogInformation("Order {OrderId} is fulfilled", orderFulfilled.OrderId);
        }

        return Ok();
    }

    // Dapr subscription for order-backordered topic
    [HttpPost("/dapr/subscribe/order-backordered")]
    [Topic("pubsub", "order-backordered")]
    public async Task<IActionResult> OnOrderBackordered(
        [FromBody] OrderBackorderedEventDto orderBackordered,
        [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("Received order-backordered event for order {OrderId}, product {ProductId}, quantity {Quantity}", orderBackordered.OrderId, orderBackordered.ProductId, orderBackordered.Quantity);

        // Logic to handle backordered orders can be added here if needed
        var order = _orderService.GetById(orderBackordered.OrderId);
        if (order is null)
        {
            _logger.LogWarning("Order not found for backorder: {OrderId}", orderBackordered.OrderId);
            return NotFound();
        }
        else
        {
            _orderService.UpdateStatus(order.Id, "pending");
            _logger.LogInformation("Order {OrderId} is backordered", orderBackordered.OrderId);
        }

        return Ok();
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