# TerraForge Installation

## Quick Start

### Server Setup

```bash
# Clone repository
git clone https://github.com/thomassanders3888-svg/terraforge.git
cd terraforge

# Start infrastructure
docker-compose up -d

# Run server
cd server
dotnet restore
dotnet run
```

### Client Setup

1. Open Unity Hub → Add project → Select `client/TerraForge`
2. Switch to player scene
3. Press Play

### Configuration

Edit `appsettings.json`:

```json
{
  "MaxPlayers": 100,
  "WorldSeed": 12345,
  "ViewDistance": 10
}
```

## Requirements

- Unity 2023.2 LTS
- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL 15
- Redis 7

## Default Ports

- Game Server: 7777
- Web API: 5000
- PostgreSQL: 5432
- Redis: 6379

## Build

```bash
# Server
./build.sh

# Client
Unity → File → Build Settings → Build
```
