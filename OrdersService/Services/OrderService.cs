using System;
using System.Collections.Generic;
using System.Linq;
using OrdersService.Models;

namespace OrdersService.Services;

public class OrderService
{
    // Example orders based on readme.md
    private static readonly List<Order> _orders = new()
    {
        new Order
        {
            Id = "o001",
            ProductId = "p002",
            Quantity = 1,
            Status = "placed",
            CreatedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc)
        },
        new Order
        {
            Id = "o002",
            ProductId = "p001",
            Quantity = 15,
            Status = "pending",
            CreatedAt = new DateTime(2024, 6, 2, 12, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 3, 9, 15, 0, DateTimeKind.Utc)
        },
        new Order
        {
            Id = "o003",
            ProductId = "p001",
            Quantity = 6,
            Status = "fulfilled",
            CreatedAt = new DateTime(2024, 6, 3, 14, 45, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 5, 16, 0, 0, DateTimeKind.Utc)
        }
    };

    public IEnumerable<Order> GetAll() => _orders;

    public Order? GetById(string orderId) =>
        _orders.FirstOrDefault(o => o.Id.Equals(orderId, StringComparison.OrdinalIgnoreCase));

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
        return order;
    }

    public bool UpdateStatus(string orderId, string status)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
        }
        return order is not null;
    }
}