using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OrdersService.Models;

namespace OrdersService.Services;

public class OrderService
{
    private readonly ILogger<OrderService> _logger;
    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    // Example orders based on readme.md
    private static readonly List<Order> _orders = new()
    {
        new Order
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = "p002",
            Quantity = 1,
            Status = "placed",
            CreatedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc)
        },
        new Order
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = "p001",
            Quantity = 15,
            Status = "pending",
            CreatedAt = new DateTime(2024, 6, 2, 12, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 3, 9, 15, 0, DateTimeKind.Utc)
        },
        new Order
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = "p001",
            Quantity = 6,
            Status = "fulfilled",
            CreatedAt = new DateTime(2024, 6, 3, 14, 45, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 5, 16, 0, 0, DateTimeKind.Utc)
        }
    };

    public IEnumerable<Order> GetAll()
    {
        _logger.LogDebug("Retrieving all orders");
        return _orders;
    }

    public Order? GetById(string orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id.Equals(orderId, StringComparison.OrdinalIgnoreCase));
        if (order is null)
            _logger.LogWarning("Order not found: {OrderId}", orderId);
        else
            _logger.LogDebug("Order found: {OrderId}", orderId);
        return order;
    }

    public Order Create(string productId, int quantity)
    {
        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            Quantity = quantity,
            Status = "placed",
            CreatedAt = now,
            UpdatedAt = now
        };
        _orders.Add(order);
        _logger.LogInformation("Created new order {OrderId} for product {ProductId} with quantity {Quantity}", order.Id, productId, quantity);
        return order;
    }

    public bool UpdateStatus(string orderId, string status)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            _logger.LogInformation("Updating status for order {OrderId} to {Status}", orderId, status);
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _logger.LogWarning("Order not found for status update: {OrderId}", orderId);
        }
        return order is not null;
    }
}