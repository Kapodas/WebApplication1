using WebApplication1.Controllers;
using WebApplication1.Services;
using WebApplication1.WebSocketHelpers;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<NoiseService>();
builder.Services.AddSingleton<RestorationService>();
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<ImageProcessingService>();
builder.Services.AddSingleton<WebSocketController>();

var app = builder.Build();

app.UseWebSockets();

var webSocketController = app.Services.GetRequiredService<WebSocketController>();

app.Map("/ws", webSocketController.HandleWebSocket);
app.MapGet("/", () => "Hello World!");

app.Run();