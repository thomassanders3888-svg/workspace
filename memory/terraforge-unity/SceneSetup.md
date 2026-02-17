# TerraForge Unity Scene Setup

## Scene Hierarchy

```
GameWorld (Scene)
├── [DONT_DESTROY] Managers
│   ├── GameManager (GameManager.cs, AudioManager.cs)
│   ├── NetworkManager (NetworkClient.cs, TerraForgeHub.cs)
│   ├── UIManager (UIManager.cs, LoadingScreen.cs)
│   ├── InputManager (InputManager.cs)
│   └── SaveManager (SaveManager.cs, WorldPersistence.cs)
├── Environment
│   ├── DirectionalLight (Sun - DayNightCycle.cs)
│   ├── TerrainManager (TerrainManager.cs, ChunkRenderer.cs)
│   └── WorldGenerator (WorldGenerator.cs, BiomeManager.cs)
├── Players
│   └── LocalPlayer (PlayerController.cs, Player.cs, CameraController.cs)
├── Entities
├── UI
│   ├── Canvas - Main Menu (UIManager.cs, SettingsMenu.cs)
│   ├── Canvas - HUD (HUDController.cs, Crosshair)
│   ├── Canvas - Inventory (InventoryUI.cs)
│   └── Canvas - Crafting (CraftingUI.cs)
└── Effects
    ├── WeatherParticles (WeatherSystem.cs)
    └── AudioSources (AudioManager.cs, SpatialAudioManager.cs)
```

## Prefab Definitions

### Player.prefab
| Component | Script | Settings |
|-----------|--------|----------|
| Transform | - | Position: 128, 70, 128 |
| CharacterController | - | Height: 1.8, Radius: 0.3 |
| PlayerController | PlayerController.cs | Speed: 4.3, Sprint: 5.6 |
| Player | Player.cs | Health: 100 |
| GravityPhysics | GravityPhysics.cs | Gravity: -20 |
| Camera (child) | CameraController.cs | FOV: 60 |

### Chunk.prefab
| Component | Script | Settings |
|-----------|--------|----------|
| Transform | - | Scale: 1,1,1 |
| MeshFilter | ChunkRenderer.cs | Procedural mesh |
| MeshCollider | ChunkRenderer.cs | Convex: false |
| ChunkData | ChunkData.cs | X, Z: runtime set |

### RemotePlayer.prefab
| Component | Script |
|-----------|--------|
| CharacterController | - |
| RemotePlayer | RemotePlayer.cs |
| Renderer | Voxel shader |
| NameTag (UI) | - |

## Component Mapping

| System | Primary Script | Dependencies |
|--------|---------------|--------------|
| Movement | PlayerController.cs | GravityPhysics.cs, InputManager.cs |
| Camera | CameraController.cs | PlayerController.cs |
| Terrain | TerrainManager.cs | ChunkRenderer.cs, WorldGenerator.cs |
| Networking | NetworkClient.cs | MultiplayerSync.cs, TerraForgeHub.cs |
| UI | UIManager.cs | HUDController.cs, InventoryUI.cs, CraftingUI.cs |
| Audio | AudioManager.cs | SpatialAudioManager.cs |
| Save | WorldPersistence.cs | SaveManager.cs |

## Setup Steps

1. Create empty scene
2. Drag Managers prefab
3. Add TerrainManager with WorldGenerator reference
4. Add Player prefab at spawn
5. Add UI Canvas with all panels
6. Configure NetworkManager with server endpoint
