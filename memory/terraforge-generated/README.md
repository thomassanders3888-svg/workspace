# TerraForge

A multiplayer voxel game inspired by Minecraft and Wurm Online.

## Features

- **Voxel Engine**: Chunk-based terrain with infinite world generation
- **Multiplayer**: Real-time multiplayer via SignalR/WebSocket
- **Biomes**: Plains, mountains, deserts, forests with unique terrain
- **Crafting**: Resource gathering and item crafting
- **Steam Integration**: Achievements, lobbies, rich presence
- **Day/Night Cycle**: Dynamic lighting and time progression

## Architecture

### Unity Client
- `PlayerController.cs` - First-person movement and camera
- `TerrainManager.cs` - Chunk loading and streaming
- `ChunkRenderer.cs` - Voxel mesh generation
- `NetworkClient.cs` - SignalR connection management
- `UIManager.cs` - Menus and HUD
- `GameManager.cs` - Scene coordination

### Server (.NET 8)
- SignalR Hub for real-time communication
- PostgreSQL for persistence
- Redis for caching
- Docker deployment ready

## Quick Start

```bash
# Start infrastructure
docker-compose up -d

# Run server
cd server && dotnet run

# Open Unity project
cd client && open TerraForge.sln
```

## Development

- Unity 2023 LTS
- .NET 8
- PostgreSQL 15
- Redis 7
