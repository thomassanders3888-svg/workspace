# TerraForge - MainScene Setup Documentation

## Overview
This document describes the Unity scene hierarchy and component configuration for TerraForge's MainScene.

---

## 1. Scene Hierarchy

```
MainScene
├── Directional Light
├── Main Camera
├── TerrainManager
│   └── (Runtime: Chunk objects)
├── NetworkManager
├── PlayerSpawn
│   └── (Runtime: Local Player)
├── UIManager
│   ├── Crosshair
│   ├── HUD
│   ├── InventoryUI
│   └── ChatPanel
└── GameManager
```

---

## 2. Required Prefabs

### Player Prefab
**Path:** `Prefabs/Player/Player.prefab`
- `PlayerController` (MonoBehaviour)
- `CharacterController`
- `Camera` (child object, for first-person view)
- Tag: `Player`
- Layer: `LocalPlayer`

### Chunk Prefab
**Path:** `Prefabs/Terrain/Chunk.prefab`
- `Chunk` (MonoBehaviour)
- `MeshFilter`
- `MeshRenderer`
- `MeshCollider`
- Tag: `Terrain`
- Layer: `Ground`

### RemotePlayer Prefab
**Path:** `Prefabs/Player/RemotePlayer.prefab`
- `RemotePlayerController` (MonoBehaviour)
- `CharacterController`
- `PlayerModel` (child, for third-person visibility)
- Tag: `RemotePlayer`
- Layer: `RemotePlayer`

---

## 3. Component Wiring

### TerrainGenerator
**Attached to:** `TerrainManager` GameObject

| Field | Reference | Purpose |
|-------|-----------|---------|
| `ChunkPrefab` | `Prefabs/Terrain/Chunk` | Template for chunk instantiation |
| `ChunkSize` | `16` | Voxels per chunk dimension |
| `RenderDistance` | `8` | Chunks to generate around player |
| `Seed` | `0` (randomized) | World generation seed |
| `BiomeSettings` | ScriptableObject | Biome configuration asset |

**Key Methods:**
- `GenerateChunk(Vector3Int position)` - Creates chunk at world position
- `UnloadChunk(Vector3Int position)` - Removes distant chunks

### NetworkManager
**Attached to:** `NetworkManager` GameObject

| Field | Value | Notes |
|-------|-------|-------|
| `ServerAddress` | `127.0.0.1` (default) | Can be overridden via GameManager |
| `Port` | `7777` | Default multiplayer port |
| `MaxPlayers` | `100` | Server capacity |
| `PlayerPrefab` | `Prefabs/Player/RemotePlayer` | Spawned for remote connections |

**Callbacks:**
- `OnPlayerConnected` → Spawns RemotePlayer prefab
- `OnPlayerDisconnected` → Destroys RemotePlayer instance

### PlayerController
**Attached to:** `Player` prefab (local)

| Field | Reference | Purpose |
|-------|-----------|---------|
| `CharacterController` | Self | Movement physics |
| `CameraTransform` | Child Camera | First-person view attachment |
| `TerrainManager` | `GameObject.Find("TerrainManager")` | Block interaction |
| `NetworkManager` | `GameObject.Find("NetworkManager")` | Position sync |
| `Crosshair` | `UIManager/Crosshair` | Interaction targeting |

**Input Bindings:**
- WASD: Movement
- Space: Jump
- Mouse: Camera look
- Left Click: Break block
- Right Click: Place block
- E: Open inventory

---

## 4. Lighting Setup

### Directional Light
**Name:** `Directional Light`

| Property | Setting |
|----------|---------|
| Type | Directional |
| Color | `#FFF4E5` (warm sunlight) |
| Intensity | `1.2` |
| Shadow Type | Soft Shadows |
| Shadow Resolution | `2048` |
| Shadow Distance | `150` |
| Shadow Cascades | `4` |
| Culling Mask | Everything |

**Transform:**
- Rotation: `X: 50, Y: -30, Z: 0`
- Position: `0, 1000, 0` (far above world)

---

## 5. Camera Setup

### Main Camera
**Name:** `Main Camera`

| Property | Setting |
|----------|---------|
| Clear Flags | Solid Color |
| Background Color | `#87CEEB` (sky blue) |
| Projection | Perspective |
| Field of View | `70` |
| Clipping Planes | Near: `0.01`, Far: `1000` |
| Depth | `-1` |
| Rendering Path | Forward |
| Occlusion Culling | Enabled |

### CameraController (Script)
**Attached to:** `Main Camera` GameObject

| Field | Reference |
|-------|-----------|
| `Target` | Player's CameraTransform anchor |
| `MouseSensitivity` | `2.0` |
| `SmoothTime` | `0.05` |
| `MinVerticalAngle` | `-89` |
| `MaxVerticalAngle` | `89` |

---

## Quick Reference

### Runtime Object Spawning Order
1. `GameManager` initializes
2. `NetworkManager` connects/spawns
3. `TerrainManager` loads initial chunks
4. `PlayerController` instantiates at `PlayerSpawn`
5. `UIManager` binds to player events

### Layer Collision Matrix
| Layer | Collides With |
|-------|---------------|
| LocalPlayer | Ground, Terrain |
| RemotePlayer | Ground, Terrain |
| Ground | All |
