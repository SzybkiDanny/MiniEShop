using MiniEShop.Notifications.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Service Bus settings
builder.Services.Configure<ServiceBusSettings>(builder.Configuration.GetSection("ServiceBus"));

// Add background service
builder.Services.AddHostedService<NotificationProcessor>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Run();
