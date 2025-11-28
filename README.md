# Vainglory-Inspired MOBA Prototype

A mobile MOBA prototype for iOS inspired by Vainglory, built in Unity. This project focuses on authentic touch controls and responsive game feel with placeholder 2D assets, designed for incremental 3D asset replacement.

## Project Status

This is a **Phase 1-5 prototype** with all core gameplay systems implemented:

- [x] Touch input system (tap-to-move, ability targeting, drag gestures)
- [x] Smooth camera controller
- [x] Character movement with NavMesh pathfinding
- [x] Complete hero: Ringo with 4 abilities
- [x] HUD and ability button UI
- [x] Halcyon Fold-style map (single lane + jungle)
- [x] Turrets and Vain Crystals
- [x] Minion spawning and AI
- [x] Jungle camps
- [x] Basic shop system
- [x] Enemy AI opponent
- [x] Death and respawn system

**Phase 6 (3D Assets)** is pending - see `docs/plans/gemini-asset-specs.md` for asset requirements.

## Requirements

- **Unity 2022.3 LTS** or newer
- **AI Navigation package** (for NavMesh)
- **iOS Build Support** module
- Xcode (for iOS deployment)

## Quick Start

### 1. Open in Unity

1. Open Unity Hub
2. Click "Add" and select the `vainglory-moba` folder
3. Open the project with Unity 2022.3 LTS or newer

### 2. Install Required Packages

The project requires the AI Navigation package. Unity should prompt you to install it, or:

1. Go to **Window > Package Manager**
2. Click **+** > **Add package by name**
3. Enter: `com.unity.ai.navigation`
4. Click **Add**

### 3. Create the Game Scene

1. Create a new scene: **File > New Scene**
2. Delete the default Main Camera and Directional Light
3. Create an empty GameObject named "GameSetup"
4. Add the `GameSceneSetup` component to it
5. Save the scene as `Assets/Scenes/GameScene.unity`

### 4. Bake NavMesh

1. Open **Window > AI > Navigation**
2. Go to the **Bake** tab
3. Click **Bake**

### 5. Play!

Press Play in the Unity Editor to test the game.

## Controls

### Touch (iOS/Mobile)
- **Tap ground**: Move to location
- **Tap and hold**: Continuous movement following finger
- **Tap enemy**: Auto-attack
- **Tap ability button**: Start targeting mode
- **Drag from ability**: Aim and release to cast
- **Pinch**: Zoom camera

### Mouse (Editor Testing)
- **Left click ground**: Move to location
- **Left click enemy**: Auto-attack
- **Click ability buttons**: Use abilities
- **Scroll wheel**: Zoom camera

### Keyboard Shortcuts
- **Q/W/E/R**: Abilities 1-4
- **B**: Toggle shop
- **ESC**: Close shop

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, Input, Camera
│   ├── Characters/     # Hero, Stats, Motor, AI
│   ├── Combat/         # Abilities, Projectiles, Items
│   ├── Map/            # Turrets, Minions, Jungle
│   ├── UI/             # HUD, Shop, Health Bars
│   └── Effects/        # Visual effects, Indicators
├── Prefabs/
├── Scenes/
├── Materials/
└── Textures/
```

## Ringo's Abilities

| Ability | Key | Description |
|---------|-----|-------------|
| **Passive: Double Down** | - | Every 6s, next attack deals 80% bonus damage |
| **A: Achilles Shot** | Q | Skillshot that damages and slows |
| **B: Twirling Silver** | W | Attack speed buff, attacks reduce cooldowns |
| **Ultimate: Hellfire Brew** | R | Homing fireball, can be body-blocked |

## Game Mechanics

### Map Layout
- Single lane (like Halcyon Fold 3v3)
- 2 turrets per side + Vain Crystal
- Jungle on both sides with 3 camps each

### Win Condition
Destroy the enemy Vain Crystal (protected until both turrets fall).

### Economy
- Start with 500 gold
- Minions give 20 gold each
- Jungle camps give 30-100 gold
- Shop at base only

## iOS Deployment

1. **Build Settings**: File > Build Settings > iOS
2. **Player Settings**:
   - Bundle Identifier: `com.yourcompany.vainglory`
   - Target minimum iOS: 12.0
3. **Build**: Creates Xcode project
4. **Xcode**: Open, sign with your team, build to device

## Next Steps

### Phase 6: 3D Assets
See `docs/plans/gemini-asset-specs.md` for detailed asset specifications to generate with AI tools or source from Unity Asset Store.

### Future Phases
- Second hero
- Multiplayer (Photon/Netcode)
- Audio system
- Tutorial
- Matchmaking

## Architecture Notes

### Input System
`TouchInputManager` handles all touch/mouse input and fires events. Supports:
- Tap detection with threshold
- Drag gestures
- UI exclusion
- Screen-to-world conversion

### Movement
`CharacterMotor` uses NavMeshAgent with custom smoothing on top for responsive, non-robotic movement.

### Abilities
All abilities inherit from `AbilityBase` which handles:
- Cooldowns
- Energy costs
- Targeting validation
- Damage calculation

### AI
`AIController` makes periodic decisions based on:
- Health state (retreat if low)
- Enemy proximity
- Farm opportunities
- Ability availability

## Known Limitations

- Placeholder visuals (2D shapes)
- No audio
- Single hero (Ringo only)
- No multiplayer
- NavMesh must be baked in editor

## Credits

Inspired by Vainglory by Super Evil Megacorp.

This is a fan-made prototype for educational purposes only.
