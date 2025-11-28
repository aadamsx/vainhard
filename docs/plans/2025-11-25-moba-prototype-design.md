# Vainglory-Inspired MOBA Prototype Design

## Overview

A mobile MOBA prototype for iOS inspired by Vainglory, focusing on authentic touch controls and responsive game feel. Built in Unity with 2D placeholder assets, designed for incremental 3D asset replacement.

**Target:** iOS, 60fps, iPhone X+
**Engine:** Unity 2022 LTS + Universal Render Pipeline (URP)

---

## Project Structure

```
vainglory-moba/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # Game manager, input system, camera
│   │   ├── Characters/     # Hero base class, abilities
│   │   ├── Combat/         # Damage, projectiles, targeting
│   │   ├── Map/            # Lane, jungle, turrets, minions
│   │   └── UI/             # HUD, ability buttons, health bars
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Materials/
│   └── Textures/
├── docs/
│   └── plans/
```

**Architecture Principles:**
- Input handling decoupled from game logic
- All movement uses interpolation (no teleporting)
- Fixed timestep for physics, variable for rendering
- Event-driven communication between systems

---

## Touch Control System

### Movement (Tap-to-Move)
- Tap anywhere on map: hero pathfinds to that point
- Tap and hold: hero follows finger continuously
- Visual feedback: ring indicator at tap location
- Smooth acceleration/deceleration curves

### Ability Controls (Bottom HUD)
- 3 ability buttons + 1 ultimate
- **Tap ability:** activates if no target needed (self-buff)
- **Tap ability then tap map:** targeted ability fires to location
- **Drag from ability to map:** skillshot aiming with range indicator
- **Cancel:** drag back to button or tap elsewhere

### Attack
- Tap enemy: auto-attack, hero moves into range
- Hold on enemy: continuous attack focus

### Camera
- Locked to hero with smooth follow
- Slight drag/offset in movement direction
- Two-finger pinch to zoom (optional)

### Feel Requirements
- 16ms input sampling (60fps)
- Visual feedback on every touch (ripple effect)
- Ability range indicators appear instantly on touch-down
- No input delay - actions queue but feel immediate

---

## Hero: Ringo

Weapon-power marksman. First hero to implement.

### Base Stats
| Stat | Value |
|------|-------|
| Health | 700 |
| Energy | 200 |
| Weapon Power | 75 |
| Attack Speed | 1.0/sec |
| Move Speed | 3.3 |
| Attack Range | 6 units |

### Abilities

**Passive - Double Down**
- Every 6 seconds, next basic attack deals 80% bonus damage
- Visual: hand glows, audio cue when ready

**A - Achilles Shot**
- Skillshot, fires bullet in a line
- 80 damage + 30% slow for 2 seconds
- Range: 8 units
- Cooldown: 8s | Energy: 40

**B - Twirling Silver**
- Self-buff, instant cast
- +50% attack speed for 6 seconds
- Basic attacks reduce other cooldowns by 0.5s
- Cooldown: 12s | Energy: 60

**Ultimate - Hellfire Brew**
- Point-and-click on enemy hero
- Homing fireball, can be body-blocked
- 250 damage + 50 burn over 3 seconds
- Global range
- Cooldown: 60s | Energy: 100

### Placeholder Visuals
- Blue circle for Ringo
- Colored particles for each ability
- Range indicators as transparent circles

---

## Map: Halcyon Fold (3v3)

### Layout

```
        [Blue Crystal]
              |
          [Turret]
              |
          [Turret]
              |
  JUNGLE ===LANE=== JUNGLE
              |
          [Turret]
              |
          [Turret]
              |
        [Red Crystal]
```

### Dimensions
- Total map: ~80x60 units
- Lane length: ~60 units
- Jungle depth: ~15 units per side

### Lane
- Single straight lane connecting bases
- 2 turrets per side
- Minions spawn every 30 seconds

### Jungle (both sides)
- 3 camps per side: small, medium, large
- Respawn 60 seconds after cleared

---

## Structures

### Turrets
| Property | Value |
|----------|-------|
| Health | 3000 |
| Damage | 200/shot |
| Attack Speed | 1/sec |
| Targeting | Minions first, then heroes if hero attacks ally |
| Requirement | Must kill outer before inner |

### Vain Crystal
| Property | Value |
|----------|-------|
| Health | 5000 |
| Win Condition | Destroy to win |
| Protection | Invulnerable until both turrets down |

### Placeholder Visuals
- Turrets: squares with range circles
- Crystals: large diamonds

---

## Game Systems

### Minions
- Wave: 3 melee + 1 ranged, every 30 seconds
- Melee: 400 HP, 25 damage, 20 gold
- Ranged: 250 HP, 25 damage, 20 gold
- AI: walk lane, attack enemies in range

### Jungle Monsters
| Camp | HP | Gold | Bonus |
|------|-----|------|-------|
| Small (x2) | 200 each | 30 | - |
| Medium | 600 | 60 | - |
| Large | 1000 | 100 | +10% attack speed, 60s |

### Gold & Items (Simplified)
- Starting gold: 500
- Shop at base only

| Item | Weapon Power | Cost |
|------|--------------|------|
| Weapon Blade | +20 | 300g |
| Heavy Steel | +50 | 1200g |
| Sorrowblade | +150 | 3000g |

### Health & Energy
- Slow regeneration over time
- Full heal in base area
- No recall (walk back)

### Death & Respawn
- Timer: 10 + (2 × level) seconds
- Respawn at base crystal
- No gold loss

---

## Development Phases

### Phase 1 - Foundation
- Unity project structure
- Input system (tap-to-move, ability targeting)
- Camera controller
- Basic character movement with interpolation
- Placeholder hero (circle)

### Phase 2 - Combat & Ringo
- Ringo's 4 abilities
- Basic attack system
- Damage, health, energy
- Death and respawn

### Phase 3 - Map & Structures
- Halcyon Fold layout
- Turrets with AI targeting
- Vain crystals
- Navigation mesh

### Phase 4 - AI & Minions
- Minion spawning and waves
- Minion AI
- Jungle camps

### Phase 5 - Polish & Second Hero
- Shop system
- UI polish
- Second hero
- Win/lose conditions

### Phase 6 - 3D Assets
- Replace placeholders with models
- Animations
- VFX and audio

---

## Technical Requirements

### Performance
- 60fps minimum on iPhone X
- Input latency <16ms
- Smooth interpolation on all movement

### Input
- Multi-touch support
- Gesture recognition (tap, hold, drag)
- Touch event queuing

### Rendering
- URP for mobile optimization
- LOD system ready for 3D assets
- Particle system for ability effects

---

## Asset Pipeline

### Placeholder (Phase 1-5)
- 2D sprites and shapes in 3D space
- Unity primitives for map geometry
- Particle effects for feedback

### 3D Assets (Phase 6)
- Characters: Mixamo or Unity Asset Store
- Textures: AI-generated (Gemini)
- UI: AI-generated sprites (Gemini)
- See `docs/plans/gemini-asset-specs.md` for details
