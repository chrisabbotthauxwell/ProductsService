namespace OrdersService.Dtos;

public class OrderCreateDto
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
} 