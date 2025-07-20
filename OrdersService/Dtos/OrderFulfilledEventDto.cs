namespace OrdersService.Dtos;

public class OrderFulfilledEventDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime FulfilledDateTime { get; set; }
}