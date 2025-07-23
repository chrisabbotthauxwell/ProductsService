using System;
using System.Linq;
using ProductsService.Models;
using ProductsService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ProductServiceTests
{
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _loggerMock = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_loggerMock.Object);
    }

    [Fact]
    public void GetAll_ReturnsProducts()
    {
        var products = _productService.GetAll();
        Assert.NotNull(products);
        Assert.True(products.Any());
    }

    [Fact]
    public void GetById_ReturnsCorrectProduct()
    {
        var product = _productService.GetById("p001");
        Assert.NotNull(product);
        Assert.Equal("p001", product.Id);
        Assert.Equal("Wireless Mouse", product.Name);
    }

    [Fact]
    public void GetById_ReturnsNullForMissingProduct()
    {
        var product = _productService.GetById("nonexistent-id");
        Assert.Null(product);
    }

    [Fact]
    public void UpdateStock_UpdatesStockCount()
    {
        var result = _productService.UpdateStock("p002", 42);
        Assert.True(result);
        var updated = _productService.GetById("p002");
        Assert.Equal(42, updated.StockCount);
    }

    [Fact]
    public void UpdateStock_ReturnsFalseForMissingProduct()
    {
        var result = _productService.UpdateStock("nonexistent-id", 99);
        Assert.False(result);
    }
}
