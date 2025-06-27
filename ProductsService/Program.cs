var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
