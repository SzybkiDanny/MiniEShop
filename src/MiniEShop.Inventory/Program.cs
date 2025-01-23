using MediatR;
using MiniEShop.Inventory.Domain.Repositories;
using MiniEShop.Inventory.Infrastructure.Repositories;

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
builder.Services.AddSingleton<IInventoryRepository>(sp => 
    new InventoryRepository(
        storageConnectionString,
        sp.GetRequiredService<ILogger<InventoryRepository>>()));

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
