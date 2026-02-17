# TerraForge Scene Hierarchy

Complete GameObject structure for the main game scene.

---

## Root Objects

```
TerraForgeScene (Scene Root)
│
├── Managers/
│   ├── GameManager
│   ├── NetworkManager
│   ├── SaveManager
│   └── UIManager
│
├── World/
│   ├── WorldGenerator
│   ├── ChunkManager
│   ├── BiomeManager
│   ├── LightingManager
│   └── WaterManager
│
├── Entities/
│   ├── Player (Player.prefab)
│   ├── RemotePlayers/
│   │   └── [Spawned RemotePlayer.prefab instances]
│   └── NPCs/
│       └── [NPC prefab instances]
│
├── Environment/
│   ├── Directional Light (Sun)
│   ├── Ambient Light
│   └── Skybox
│
├── UI/
│   ├── Canvas (World Space)
│   ├── Canvas (Screen Space - Overlay)
│   │   ├── HUD/
│   │   │   ├── Crosshair
│   │   │   ├── Block Selector
│   │   │   ├── Health Bar
│   │   │   └── Hotbar
│   │   ├── PauseMenu
│   │   ├── Inventory
│   │   └── Chat/
│   │       ├── ChatPanel
│   │       ├── MessageList
│   │       └── InputField
│   └── BlockHighlighter (BlockHighlighter.prefab)
│
├── Audio/
│   ├── MusicSource
│   ├── AmbientSource
│   └── SFXSource
│
├── Pools/
│   ├── ChunkPool (inactive chunks)
│   └── ParticlePool
│
└── Debug/
    ├── DebugCamera
    └── GizmosHelper
```

---

## Component Mappings

### GameManager
```yaml
GameManager:
  components:
    - GameManager (Script)
    - Singleton<GameManager>
  responsibilities:
    - Game state management
    - Scene transitions
    - Pause/Resume
```

---

### NetworkManager
```yaml
NetworkManager:
  components:
    - NetworkManager (Mirror)
    - NetworkDiscovery
    - TerraForgeNetworkManager (Custom)
  serializedFields:
    playerPrefab: RemotePlayer.prefab
    autoCreatePlayer: false
    playerSpawnMethod: Random
```

---

### ChunkManager
```yaml
ChunkManager:
  components:
    - ChunkManager (Script)
    - ObjectPool
  serializedFields:
    chunkPrefab: Chunk.prefab
    viewDistance: 8
    chunkUnloadDelay: 5.0
    maxChunksPerFrame: 4
    workerThreads: 4
```

---

### WorldGenerator
```yaml
WorldGenerator:
  components:
    - WorldGenerator (Script)
    - NoiseGenerator
    - BiomeGenerator
    - StructureGenerator
  serializedFields:
    seed: 0 (random if 0)
    biomeScale: 100.0
    heightScale: 1.0
    caveEnabled: true
    oreEnabled: true
```

---

### Player (Instance)
```yaml
Player (Prefab Instance):
  sourcePrefab: Player.prefab
  instanceOverrides:
    - Transform.position = Spawn point
  runtimeComponents:
    - NetworkIdentity (added if multiplayer)
    - NetworkTransform (added if multiplayer)
```

---

### BlockHighlighter (Instance)
```yaml
BlockHighlighter (Prefab Instance):
  sourcePrefab: BlockHighlighter.prefab
  parent: Canvas/Overlay
  serializedFields:
    playerController: {reference: "Player/PlayerController"}
```

---

## Component Reference Table

| GameObject | Component | Reference To | Purpose |
|------------|-----------|------------|---------|
| Player | PlayerController | CameraController | Movement input handling |
| Player | GravityPhysics | CharacterController | Physics simulation |
| CameraHolder | CameraController | PlayerController | Camera movement sync |
| BlockHighlighter | BlockHighlighter | PlayerController.BlockTarget | Block selection highlight |
| ChunkManager | ChunkManager | WorldGenerator | Chunk coordinate generation |
| NetworkManager | TerraForgeNetworkManager | GameManager | Network state sync |
| UIManager | UIManager | GameManager | UI state management |
| SaveManager | SaveManager | ChunkManager | World persistence |

---

## Spawn Hierarchy

### Runtime Spawned Objects

```
World/ (Runtime)
└── Chunks/
    └── chunk_[x]_[y]_[z] (Chunk.prefab instances)
        └── blocks[4096] (Block data, not GameObjects)

Entities/ (Runtime)
└── RemotePlayers/
    └── remoteplayer_[netId] (RemotePlayer.prefab instances)

Pools/ (Runtime)
└── Inactive/
    └── [Pooled chunks awaiting reuse]
```

---

## Light Settings

### Directional Light (Sun)
```yaml
Light:
  type: Directional
  intensity: 1.25
  color: {r: 1, g: 0.95, b: 0.85, a: 1}
  shadowType: SoftShadows
  shadowStrength: 0.8
  shadowBias: 0.05
  shadowNormalBias: 0.4
  shadowNearPlane: 0.2
```

### Ambient Light
```yaml
RenderSettings:
  ambientMode: Gradient
  ambientSkyColor: {r: 0.5, g: 0.6, b: 0.8, a: 1}
  ambientEquatorColor: {r: 0.3, g: 0.4, b: 0.5, a: 1}
  ambientGroundColor: {r: 0.2, g: 0.2, b: 0.2,