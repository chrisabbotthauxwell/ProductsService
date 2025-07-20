namespace OrdersService.Dtos;

public class StockUpdatedEventDto
{
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}