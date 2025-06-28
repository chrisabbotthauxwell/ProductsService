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
    public IActionResult UpdateStock(string id, [FromBody] ProductUpdateDto update)
    {
        _productService.UpdateStock(id, update.StockCount);
        return NoContent();
    }
}
