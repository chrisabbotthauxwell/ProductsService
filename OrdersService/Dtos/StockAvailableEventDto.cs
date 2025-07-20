namespace OrdersService.Dtos;

public class StockAvailableEventDto
{
    public string ProductId { get; set; }
    public int StockCount { get; set; }
}