using System;

namespace ProductsService.Dtos;

public class StockAvailableEventDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int StockCount { get; set; }
    public DateTime RestockedDateTime { get; set; }
}
