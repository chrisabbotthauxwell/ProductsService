using System;

namespace ProductsService.Models;

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public bool InStock => StockCount > 0;
    public int StockCount { get; set; }
}
