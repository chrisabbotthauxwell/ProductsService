using System;
using Microsoft.Extensions.Logging;

using ProductsService.Models;

namespace ProductsService.Services;

public class ProductService
{
    private readonly ILogger<ProductService> _logger;
    public ProductService(ILogger<ProductService> logger)
    {
        _logger = logger;
    }

    private static readonly List<Product> _products = new()
    {
        new Product { Id = "p001", Name = "Wireless Mouse", StockCount = 0 },
        new Product { Id = "p002", Name = "Mechanical Keyboard", StockCount = 20 }
    };

    public IEnumerable<Product> GetAll()
    {
        _logger.LogDebug("Retrieving all products");
        return _products;
    }

    public Product? GetById(string id)
    {
        var product = _products.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (product is null)
            _logger.LogWarning("Product not found: {ProductId}", id);
        else
            _logger.LogDebug("Product found: {ProductId}", id);
        return product;
    }

    public bool UpdateStock(string id, int newStockCount)
    {
        var product = GetById(id);
        if (product is not null)
        {
            _logger.LogInformation("Updating stock for product {ProductId} to {StockCount}", id, newStockCount);
            product.StockCount = newStockCount;
        }
        else
        {
            _logger.LogWarning("Product not found for stock update: {ProductId}", id);
        }
        return product is not null;
    }
}