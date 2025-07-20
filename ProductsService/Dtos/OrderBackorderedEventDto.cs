namespace ProductsService.Dtos;

public class OrderBackorderedEventDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime BackorderedDateTime { get; set; }
}