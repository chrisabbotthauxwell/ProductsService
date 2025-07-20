using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductsService.Dtos;
using ProductsService.Models;
using ProductsService.Services;

namespace ProductsService.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        _logger.LogInformation("Getting all products");
        return Ok(_productService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(string id)
    {
        _logger.LogInformation("Getting product by id: {ProductId}", id);
        var product = _productService.GetById(id);
        if (product is null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(
            string id,
            [FromBody] ProductUpdateDto update,
            [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("Updating stock for product {ProductId} to {StockCount}", id, update.StockCount);
        try
        {
            // update stock for product
            if (_productService.UpdateStock(id, update.StockCount))
            {
                // get the updated product to check if it is now in stock
                var product = _productService.GetById(id);

                // If the product is in stock, publish a StockAvailableEvent
                if (product is not null && product.InStock)
                {
                    var stockAvailableEvent = new StockAvailableEventDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        StockCount = product.StockCount,
                        RestockedDateTime = DateTime.UtcNow
                    };

                    await daprClient.PublishEventAsync("pubsub", "stock-available", stockAvailableEvent);
                    _logger.LogInformation("Published stock-available event for product {ProductId}", product.Id);
                }
                return NoContent();
            }
            else
            {
                _logger.LogWarning("Product not found for stock update: {ProductId}", id);
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", id);
            throw;
        }
    }

    // Dapr subscription for stock-updated topic
    [HttpPost("/dapr/subscribe/stock-updated")]
    [Topic("pubsub", "stock-updated")]
    public IActionResult OnStockUpdated([FromBody] StockUpdatedEventDto stockUpdated)
    {
        _logger.LogInformation("Received stock-updated event for product {ProductId}, quantity {Quantity}, order {OrderId}", stockUpdated.ProductId, stockUpdated.Quantity, stockUpdated.OrderId);

        var product = _productService.GetById(stockUpdated.ProductId);
        if (product is null)
        {
            _logger.LogWarning("Product not found for stock update: {ProductId}", stockUpdated.ProductId);
            return NotFound();
        }

        // Decrease stock count by the fulfilled quantity
        var newStockCount = product.StockCount - stockUpdated.Quantity;
        if (newStockCount < 0) newStockCount = 0;
        _productService.UpdateStock(product.Id, newStockCount);

        _logger.LogInformation("Product {ProductId} stock decreased by {Quantity}, new stock: {StockCount}", product.Id, stockUpdated.Quantity, newStockCount);

        return Ok();
    }

    // Dapr subscription for order-placed topic
    [HttpPost("/dapr/subscribe/order-placed")]
    [Topic("pubsub", "order-placed")]
    public async Task<IActionResult> OnOrderPlaced(
        [FromBody] OrderPlacedEventDto orderPlaced,
        [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("Received order-placed event for order {OrderId}, product {ProductId}, quantity {Quantity}", orderPlaced.OrderId, orderPlaced.ProductId, orderPlaced.Quantity);

        var product = _productService.GetById(orderPlaced.ProductId);
        if (product is null)
        {
            _logger.LogWarning("Product not found for order: {ProductId}", orderPlaced.ProductId);
            return NotFound();
        }

        // Check if the product is in stock
        if (product.InStock)
        {
            if (product.StockCount >= orderPlaced.Quantity)
            {
                _logger.LogInformation("Product {ProductId} is in stock for order {OrderId}", product.Id, orderPlaced.OrderId);

                // Decrease stock count by the ordered quantity
                var newStockCount = product.StockCount - orderPlaced.Quantity;
                if (newStockCount < 0) newStockCount = 0;
                _productService.UpdateStock(product.Id, newStockCount);

                _logger.LogInformation("Product {ProductId} stock decreased by {Quantity}, new stock: {StockCount}", product.Id, orderPlaced.Quantity, newStockCount);

                // Publish order-fulfilled event
                var orderFulfilledEvent = new OrderFulfilledEventDto
                {
                    OrderId = orderPlaced.OrderId,
                    ProductId = product.Id,
                    Quantity = orderPlaced.Quantity,
                    FulfilledDateTime = DateTime.UtcNow
                };
                await daprClient.PublishEventAsync("pubsub", "order-fulfilled", orderFulfilledEvent);
                _logger.LogInformation("Published order-fulfilled event for order {OrderId}", orderPlaced.OrderId);
            }
            else
            {
                _logger.LogWarning("Not enough stock for order {OrderId}, available stock: {StockCount}", orderPlaced.OrderId, product.StockCount);

                // Publish order-backordered event
                var orderBackorderedEvent = new OrderBackorderedEventDto
                {
                    OrderId = orderPlaced.OrderId,
                    ProductId = product.Id,
                    Quantity = orderPlaced.Quantity,
                    BackorderedDateTime = DateTime.UtcNow
                };
                await daprClient.PublishEventAsync("pubsub", "order-backordered", orderBackorderedEvent);
                _logger.LogInformation("Published order-backordered event for order {OrderId}", orderPlaced.OrderId);
            }   
        }
        else
        {
            _logger.LogWarning("Product {ProductId} is out of stock for order {OrderId}", product.Id, orderPlaced.OrderId);

            //Publish order-backordered event
            var orderBackorderedEvent = new OrderBackorderedEventDto
            {
                OrderId = orderPlaced.OrderId,
                ProductId = product.Id,
                Quantity = orderPlaced.Quantity,
                BackorderedDateTime = DateTime.UtcNow
            };
            await daprClient.PublishEventAsync("pubsub", "order-backordered", orderBackorderedEvent);
            _logger.LogInformation("Published order-backordered event for order {OrderId}", orderPlaced.OrderId);
        }

        return Ok();
    }
}
