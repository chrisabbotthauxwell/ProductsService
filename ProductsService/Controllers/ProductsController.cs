using System.Threading.Tasks;
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
        if (_productService.UpdateStock(id, update.StockCount))
        {
            var product = _productService.GetById(id);
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
        _logger.LogWarning("Product not found for stock update: {ProductId}", id);
        return NotFound();
    }
}
