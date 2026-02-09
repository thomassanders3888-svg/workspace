# TerraForge — Wurm Online Competitor

## Vision
A modern sandbox MMO with deep terraforming, player-driven economy, and skill-based progression. Built for Steam deployment with contemporary engines.

## Why Compete with Wurm?
- Wurm Online (2006) shows market demand for hardcore sandbox MMOs
- Aging codebase limits modern features
- Niche underserved: "Hardcore crafting/terraforming MMO"
- Steam provides distribution/anti-cheat

## Technical Decisions

### Engine Comparison

| Engine | Pros | Cons | Verdict |
|--------|------|------|---------|
| **Unity 6** | Rapid prototyping, asset store, C#, WebGL builds | Performance ceiling, licensing costs at scale | **Backup** |
| **Unreal Engine 5** | Nanite/Lumen, Blueprints, AAA graphics, free until $1M | Steeper learning curve, C++ or Blueprints | **PRIMARY** |
| **Godot 4** | Free, lightweight, GDScript/C# | Smaller ecosystem, less proven for MMO | Research |
| **Custom** | Full control | 3-5 year dev time before playable | Nope |

**Decision: Unreal Engine 5**
- Reasons: Built-in networking (replication), world composition for large worlds, visual scripting speeds iteration

## Core Systems Needed

### Phase 1: Foundation (Months 1-6)
- [ ] Terrain generation (procedural continent)
- [ ] Voxel-based terraforming (raise/lower/flatten)
- [ ] Basic player controller (first/third person)
- [ ] Network architecture (dedicated servers)
- [ ] Inventory system

### Phase 2: Crafting Economy (Months 7-12)
- [ ] 100+ craftable items
- [ ] Quality/QL system (like Wurm)
- [ ] Resource nodes (trees, ore, clay)
- [ ] Tool degradation
- [ ] Player trading

### Phase 3: Progression (Months 13-18)
- [ ] Skill system (50+ skills, 1-100)
- [ ] Character stats
- [ ] Meditation/religion system
- [ ] Deity favor mechanics

### Phase 4: Social/PvP (Months 19-24)
- [ ] Village/kingdom system
- [ ] Land claims
- [ ] Optional PvP servers
- [ ] Warfare mechanics

### Phase 5: Polish/Steam (Months 25-30)
- [ ] Anti-cheat (EAC integration)
- [ ] Steamworks SDK
- [ ] Monetization design
- [ ] EA launch

## Revenue Model
- **Base Game**: $19.99 (one-time)
- **Cosmetics**: Optional skins, decorations
- **Premium**: $4.99/mo for increased limits (optional, not P2W)
- **Server Hosting**: Official + player-hosted servers

## Daily Work Plan
Each day I'll post updates on:
1. Progress made
2. Blockers encountered  
3. Next day's tasks
4. Technical decisions

## Resources to Research
- SpatialOS for MMO networking
- Unreal Engine 5.4+ features
- Procedural terrain in UE5
- Steamworks integration
- Wurm's specific mechanics (for inspiration, not copying)

## Risk Mitigation
- Start with single-player offline mode
- Add multiplayer incrementally
- Focus on "fun" before "massive"
- Regular playtesting builds

---

*Project initiated: 2026-02-08*
*Next action: Install UE5, create base project, research terrain systems*
