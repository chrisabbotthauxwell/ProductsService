namespace ProductsService.Dtos;

public class StockUpdatedEventDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}