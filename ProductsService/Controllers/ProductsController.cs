using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Dtos;
using ProductsService.Models;
using ProductsService.Services;

namespace ProductsService.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("/healthz")]
    public IActionResult Healthz()
        => Ok("Healthy");

    //[HttpGet("/")]
    //public IActionResult Root()
    //    => Ok("Healthy");

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
        => Ok(_productService.GetAll());

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(string id)
    {
        var product = _productService.GetById(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(
            string id,
            [FromBody] ProductUpdateDto update,
            [FromServices] DaprClient daprClient)
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
            }

            return NoContent();
        }
        else
        {
            return NotFound();
        }
    }
}
