using MediatR;
using MiniEShop.Orders.Domain.Repositories;
using MiniEShop.Orders.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Configure Azure Storage
var storageConnectionString = builder.Configuration["AzureStorage:ConnectionString"]
    ?? throw new InvalidOperationException("Azure Storage connection string is not configured.");

// Register repository
builder.Services.AddSingleton<IOrderRepository>(sp => 
    new OrderRepository(
        storageConnectionString,
        sp.GetRequiredService<ILogger<OrderRepository>>()));

// Configure HTTP clients
builder.Services.AddHttpClient("ProductsService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Products"]
        ?? throw new InvalidOperationException("Products service URL is not configured."));
});

builder.Services.AddHttpClient("InventoryService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Inventory"]
        ?? throw new InvalidOperationException("Inventory service URL is not configured."));
});

// Add session handling
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();

// Add session ID middleware
app.Use(async (context, next) =>
{
    if (context.Session.GetString("SessionId") == null)
    {
        context.Session.SetString("SessionId", Guid.NewGuid().ToString());
    }
    await next();
});

app.UseAuthorization();
app.MapControllers();

app.Run();
