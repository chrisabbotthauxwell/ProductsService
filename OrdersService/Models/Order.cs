using System;

namespace OrdersService.Models;

public class Order
{
    public string Id { get; set; } = default!;
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
    public string Status { get; set; } = default!; // placed, fulfilled, pending
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}