using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

public class ApiContractTests
{
    private readonly HttpClient _productsClient = new HttpClient { BaseAddress = new System.Uri("http://localhost:8080") };
    private readonly HttpClient _ordersClient = new HttpClient { BaseAddress = new System.Uri("http://localhost:8081") };

    [Fact]
    public async Task ProductsService_OpenApiSpec_ShouldContainAllEndpointsAndSchemas()
    {
        var response = await _productsClient.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        var specJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(specJson);
        var root = doc.RootElement;

        var paths = root.GetProperty("paths");
        // Check for all ProductsService endpoints
        paths.TryGetProperty("/Products", out _).Should().BeTrue();
        paths.TryGetProperty("/Products/{id}", out _).Should().BeTrue();
        paths.TryGetProperty("/Products/{id}/stock", out _).Should().BeTrue();

        // Check for Product schema
        var schemas = root.GetProperty("components").GetProperty("schemas");
        schemas.TryGetProperty("Product", out var productSchema).Should().BeTrue();
        productSchema.GetProperty("properties").TryGetProperty("id", out _).Should().BeTrue();
        productSchema.GetProperty("properties").TryGetProperty("name", out _).Should().BeTrue();
        productSchema.GetProperty("properties").TryGetProperty("stockCount", out _).Should().BeTrue();
        productSchema.GetProperty("properties").TryGetProperty("inStock", out _).Should().BeTrue();
    }

    [Fact]
    public async Task OrdersService_OpenApiSpec_ShouldContainAllEndpointsAndSchemas()
    {
        var response = await _ordersClient.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        var specJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(specJson);
        var root = doc.RootElement;

        var paths = root.GetProperty("paths");
        // Check for all OrdersService endpoints
        paths.TryGetProperty("/Orders", out _).Should().BeTrue(); // GET, POST
        paths.TryGetProperty("/Orders/{id}", out _).Should().BeTrue(); // GET by id

        // Check for Order schema
        var schemas = root.GetProperty("components").GetProperty("schemas");
        schemas.TryGetProperty("Order", out var orderSchema).Should().BeTrue();
        orderSchema.GetProperty("properties").TryGetProperty("id", out _).Should().BeTrue();
        orderSchema.GetProperty("properties").TryGetProperty("productId", out _).Should().BeTrue();
        orderSchema.GetProperty("properties").TryGetProperty("quantity", out _).Should().BeTrue();
        orderSchema.GetProperty("properties").TryGetProperty("status", out _).Should().BeTrue();
        orderSchema.GetProperty("properties").TryGetProperty("createdAt", out _).Should().BeTrue();
        orderSchema.GetProperty("properties").TryGetProperty("updatedAt", out _).Should().BeTrue();
    }
}
