using ProductsService.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddDapr();
builder.Services.AddDaprClient();

// Register the ProductService as a singleton in memory
builder.Services.AddSingleton<ProductService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights for telemetry
if (builder.Environment.IsProduction())
{
    var appInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(appInsightsConnectionString))
    {
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
        builder.Services.AddOpenTelemetry().UseAzureMonitor(options => {
            options.ConnectionString = appInsightsConnectionString;
        });
    }
}

Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {builder.Environment.EnvironmentName}");

builder.Logging.AddConsole();

//builder.Services.AddHttpLogging(logging =>
//{
//   logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// don't force HTTPS, let ACA ingress layer enforce TLS
// can check X-Forwarded-Proto if need to detect secure request
//app.UseHttpsRedirection();

app.UseCloudEvents(); //for pub/sub payloads
app.MapSubscribeHandler(); //for auto-discovery of pub/sub subscriptions
//app.UseHttpLogging(); //for logging HTTP requests

app.UseAuthorization();

app.MapControllers();

app.Run();
