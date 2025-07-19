using System;
using System.Collections.Generic;
using System.Linq;
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
        _logger.LogInformation("Retrieving all products");
        return _products;
    }

    public Product? GetById(string id)
    {
        _logger.LogInformation("Retrieving product by id: {ProductId}", id);
        return _products.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public bool UpdateStock(string id, int newStockCount)
    {
        var product = GetById(id);
        if (product is not null)
        {
            product.StockCount = newStockCount;
            _logger.LogInformation("Updated stock for product {ProductId} to {StockCount}", id, newStockCount);
        }
        else
        {
            _logger.LogWarning("Product not found for stock update: {ProductId}", id);
        }
        return product is not null;
    }
}