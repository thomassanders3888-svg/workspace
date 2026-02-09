// TerraForge Dedicated Server
// A Wurm Online-inspired sandbox MMO server

using TerraForgeServer.Core;
using TerraForgeServer.Networking;
using TerraForgeServer.World;

Console.WriteLine(@"
  _____              _____                      
 |_   _|__ _ _ _ __ |  ___|__  __ _ _ __ ___   
   | |/ _ \ '_| '_ \| |_ / _ \/ _` | '_ ` _ \  
   | |  __/ | | |_) |  _|  __/ (_| | | | | | | 
   |_|\___|_| | .__/|_|  \___|\__,_|_| |_| |_| 
              |_|                               
              Server v0.1.0
");

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services
builder.Services.AddSignalR()
    .AddMessagePackProtocol();

builder.Services.AddSingleton<GameServer>();
builder.Services.AddSingleton<WorldManager>();
builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<TerrainEngine>();

// Steam integration (if configured)
var steamAppId = builder.Configuration.GetValue<int>("STEAM_APP_ID", 0);
if (steamAppId > 0)
{
    builder.Services.AddSingleton<SteamIntegration>();
    Console.WriteLine($"[Steam] Integration enabled for App ID: {steamAppId}");
}

var app = builder.Build();

// Get core services
var gameServer = app.Services.GetRequiredService<GameServer>();
var worldManager = app.Services.GetRequiredService<WorldManager>();

// Initialize world
Console.WriteLine("[World] Initializing terrain engine...");
await worldManager.InitializeAsync();

// Start game server
Console.WriteLine("[Network] Starting game server...");
await gameServer.StartAsync();

// Map SignalR hubs
app.MapHub<GameHub>("/game");
app.MapHub<AdminHub>("/admin");

// Health check endpoint
app.MapGet("/health", () => new {
    status = "online",
    players = gameServer.PlayerCount,
    uptime = gameServer.Uptime,
    version = "0.1.0"
});

// Run application
Console.WriteLine("[Server] TerraForge is online!");
Console.WriteLine($"[Network] Game port: 7777 (UDP)");
Console.WriteLine($"[Network] Admin port: 7778 (TCP)");

await app.RunAsync();
