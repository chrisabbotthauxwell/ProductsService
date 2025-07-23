using System;
using System.Linq;
using OrdersService.Models;
using OrdersService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class OrderServiceTests
{
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _loggerMock = new Mock<ILogger<OrderService>>();
        _orderService = new OrderService(_loggerMock.Object);
    }

    [Fact]
    public void GetAll_ReturnsOrders()
    {
        var orders = _orderService.GetAll();
        Assert.NotNull(orders);
        Assert.True(orders.Any());
    }

    [Fact]
    public void Create_AddsOrder()
    {
        var order = _orderService.Create("p003", 5);
        Assert.NotNull(order);
        Assert.Equal("p003", order.ProductId);
        Assert.Equal(5, order.Quantity);
        Assert.Equal("placed", order.Status);
        Assert.True(_orderService.GetAll().Any(o => o.Id == order.Id));
    }

    [Fact]
    public void GetById_ReturnsCorrectOrder()
    {
        var created = _orderService.Create("p004", 2);
        var found = _orderService.GetById(created.Id);
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public void UpdateStatus_UpdatesOrderStatus()
    {
        var order = _orderService.Create("p005", 1);
        var result = _orderService.UpdateStatus(order.Id, "fulfilled");
        Assert.True(result);
        var updated = _orderService.GetById(order.Id);
        Assert.Equal("fulfilled", updated.Status);
    }

    [Fact]
    public void UpdateUpdatedAt_UpdatesOrderUpdatedAt()
    {
        var order = _orderService.Create("p006", 3);
        var newDate = DateTime.UtcNow.AddDays(1);
        var result = _orderService.UpdateUpdatedAt(order.Id, newDate);
        Assert.True(result);
        var updated = _orderService.GetById(order.Id);
        Assert.Equal(newDate, updated.UpdatedAt);
    }

    [Fact]
    public void GetById_ReturnsNullForMissingOrder()
    {
        var result = _orderService.GetById("nonexistent-id");
        Assert.Null(result);
    }

    [Fact]
    public void UpdateStatus_ReturnsFalseForMissingOrder()
    {
        var result = _orderService.UpdateStatus("nonexistent-id", "pending");
        Assert.False(result);
    }

    [Fact]
    public void UpdateUpdatedAt_ReturnsFalseForMissingOrder()
    {
        var result = _orderService.UpdateUpdatedAt("nonexistent-id", DateTime.UtcNow);
        Assert.False(result);
    }
}
