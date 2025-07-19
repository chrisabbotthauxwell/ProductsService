using OrdersService.Services;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("serilog.json", optional: false, reloadOnChange: true);

// Add Serilog after adding the configuration file
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
);

// Add services to the container.

builder.Services.AddControllers().AddDapr();
builder.Services.AddDaprClient();

// Register the OrderService as a singleton in memory
builder.Services.AddSingleton<OrderService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

Log.Information("OrdersService started at {Time}", DateTime.UtcNow);
app.Run();
