namespace OrdersService.Dtos;

public class StockAvailableEventDto
{
    public string ProductId { get; set; } = string.Empty;
    public int StockCount { get; set; }
}