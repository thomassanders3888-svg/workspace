# TerraForge

**A Wurm Online competitor built for modern platforms.**

## Architecture

```
terraforge/
├── client/          # Unity 6 game client
├── server/          # Dedicated game server (.NET)
├── database/        # PostgreSQL schemas
└── infra/           # Docker & deployment
```

## Quick Start

```bash
# Start entire stack
docker-compose up -d

# Develop client
cd client && unity -projectPath .

# Build server
cd server && dotnet build
```

## Tech Stack
- **Client**: Unity 6 (DOTS for ECS)
- **Server**: .NET 8 with SignalR
- **Database**: PostgreSQL + TimescaleDB
- **Networking**: Custom UDP protocol + Steam relay
- **Terrain**: DOTS-based voxel system

## Status
Phase 1: Infrastructure & server foundation
