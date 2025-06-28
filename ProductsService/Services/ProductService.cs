using System;

using ProductsService.Models;

namespace ProductsService.Services;

public class ProductService
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = "p001", Name = "Wireless Mouse", StockCount = 0 },
        new Product { Id = "p002", Name = "Mechanical Keyboard", StockCount = 20 }
    };

    public IEnumerable<Product> GetAll() => _products;

    public Product? GetById(string id) =>
        _products.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public void UpdateStock(string id, int newStockCount)
    {
        var product = GetById(id);
        if (product is not null)
        {
            product.StockCount = newStockCount;
        }
    }
}