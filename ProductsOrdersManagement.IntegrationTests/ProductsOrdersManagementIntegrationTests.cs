using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

// NOTE: These integration tests target the running Docker Compose stack.
// Before running, start the stack with: docker compose up --build
// ProductsService: http://localhost:8080
// OrdersService: http://localhost:8081
// Dapr sidecars and Redis are included in the stack.

public class ProductsOrdersManagementIntegrationTests
{
    private readonly HttpClient _productsClient;
    private readonly HttpClient _ordersClient;

    public ProductsOrdersManagementIntegrationTests()
    {
        _productsClient = new HttpClient { BaseAddress = new System.Uri("http://localhost:8080") };
        _ordersClient = new HttpClient { BaseAddress = new System.Uri("http://localhost:8081") };
    }

    [Fact]
    public async Task OrderPlacementAndFulfillment_ShouldFulfillOrder_WhenStockAvailable()
    {
        // Place an order for a product with stock
        var orderPayload = new { productId = "p002", quantity = 1 };
        var content = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");
        var response = await _ordersClient.PostAsync("/orders", content);
        response.EnsureSuccessStatusCode();
        var order = JsonSerializer.Deserialize<OrderDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert initial order status is placed
        order.Status.Should().Be("placed");

        // Check that order status is updated to fulfilled
        // Poll for status update
        OrderDto updatedOrder = null;
        var maxAttempts = 10;
        var delayMs = 500;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var getOrderResponse = await _ordersClient.GetAsync($"/orders/{order.Id}");
            getOrderResponse.EnsureSuccessStatusCode();
            updatedOrder = JsonSerializer.Deserialize<OrderDto>(await getOrderResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (updatedOrder.Status == "fulfilled")
                break;
            await Task.Delay(delayMs);
        }

        updatedOrder.Status.Should().Be("fulfilled");
    }

    [Fact]
    public async Task OrderPlacementAndBackordering_ShouldBackorder_WhenStockUnavailable()
    {
        // Place an order for a product with no stock
        var orderPayload = new { productId = "p003", quantity = 1 };
        var content = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");
        var response = await _ordersClient.PostAsync("/orders", content);
        response.EnsureSuccessStatusCode();
        var order = JsonSerializer.Deserialize<OrderDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert order status is placed
        order.Status.Should().Be("placed");

        // Check that order status is updated to pending
        // Poll for status update
        OrderDto updatedOrder = null;
        var maxAttempts = 10;
        var delayMs = 500;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var getOrderResponse = await _ordersClient.GetAsync($"/orders/{order.Id}");
            getOrderResponse.EnsureSuccessStatusCode();
            updatedOrder = JsonSerializer.Deserialize<OrderDto>(await getOrderResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (updatedOrder.Status == "pending")
                break;
            await Task.Delay(delayMs);
        }

        updatedOrder.Status.Should().Be("pending");
    }

    [Fact]
    public async Task StockUpdateAndPendingOrderFulfillment_ShouldFulfillPendingOrder_WhenStockBecomesAvailable()
    {
        // Place a pending order (no stock)
        var orderPayload = new { productId = "p001", quantity = 1 };
        var content = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");
        var orderResponse = await _ordersClient.PostAsync("/orders", content);
        orderResponse.EnsureSuccessStatusCode();
        var order = JsonSerializer.Deserialize<OrderDto>(await orderResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        order.Status.Should().Be("placed");

        // Update stock for product
        var stockPayload = new { stockCount = 10 };
        var stockContent = new StringContent(JsonSerializer.Serialize(stockPayload), Encoding.UTF8, "application/json");
        var stockResponse = await _productsClient.PutAsync($"/products/{order.ProductId}/stock", stockContent);
        stockResponse.EnsureSuccessStatusCode();

        // Re-fetch order to verify status update
        // Check that order status is updated to fulfilled
        // Poll for status update
        OrderDto updatedOrder = null;
        var maxAttempts = 10;
        var delayMs = 500;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var getOrderResponse = await _ordersClient.GetAsync($"/orders/{order.Id}");
            getOrderResponse.EnsureSuccessStatusCode();
            updatedOrder = JsonSerializer.Deserialize<OrderDto>(await getOrderResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (updatedOrder.Status == "fulfilled")
                break;
            await Task.Delay(delayMs);
        }

        updatedOrder.Status.Should().Be("fulfilled");
        
    }

    private class OrderDto
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }
}
