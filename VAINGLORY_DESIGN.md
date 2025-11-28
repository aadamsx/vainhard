# Vainglory Game Design Document

## Overview
Vainglory is a 3v3 MOBA designed for touch controls. Two teams fight to destroy the enemy's Vain Crystal. Matches last approximately 20 minutes.

## Map: Halcyon Fold

### Layout
- **Lane**: Single lane connecting both bases (top of map)
- **Jungle**: Below the lane, contains camps and objectives
- **Bases**: On opposite ends, each with Vain Crystal

### Structures Per Team
1. **Outer Turret** (first turret in lane)
2. **Inner Turret** (second turret, closer to base)
3. **Base Turret** (third turret, inside base - tankier)
4. **Vain Crystal** (core objective)

## Turrets

### Stats
- Health: 4000
- Weapon Damage: 160
- Armor/Shield: 32 + 13/level
- Attack Cooldown: 1.25 seconds
- Max Barrier: 800 (regenerates at 200/sec when not attacked)

### Behavior
- Prefers attacking minions
- Targets heroes who:
  - Enter range without friendly minions
  - Attack an enemy hero within turret range
- Damage increases per consecutive hit on same hero (45% per stack, max 6 stacks)
- Shield pierce: 50%

## Lane Minions

### Wave Composition
- **3 Melee Minions**: Tankier, shorter range
- **3 Ranged Minions**: Squishier, longer range
- **(Later) 1 Siege Minion**: Every 3rd wave after certain time

### Spawn Timing
- First wave: ~30 seconds into game
- Subsequent waves: Every 30 seconds
- Waves meet in middle of lane

### Stats (Estimated - scales with time)
| Type | Health | Damage | Gold | XP |
|------|--------|--------|------|-----|
| Melee | 450 | 12 | 22 | 30 |
| Ranged | 280 | 23 | 17 | 25 |
| Siege | 800 | 40 | 45 | 50 |

### Key Mechanic: Last Hitting
Gold bounty goes 100% to whoever deals the killing blow. This is THE core farming mechanic.

## Jungle Camps

### Types
1. **Healing Treants** (gives health back)
   - Elder Treant: 960 + 180/lvl HP, 64 gold, 27 XP
   - Small Treant: 480 + 90/lvl HP, 32 gold, 27 XP

2. **Jungle Minion Camp** (by shop)
   - Big Minion: 360 + 65/lvl HP, 75 gold, 13 XP
   - Small Minion: 300 + 60/lvl HP, 22 gold, 10 XP

3. **Crystal Miner** (in each jungle)
   - 1200 + 200/lvl HP, 125 gold, 15 XP

### Spawn Timing
- Initial spawn: 0:45
- Respawn: ~50 seconds after killed
- Level up: Every 2 minutes

## Major Objectives

### Gold Mine (4:00 - 15:00)
- Health: 1200 + 200/lvl
- Capturing team gets gold over time (up to 300 gold)
- Located center of jungle

### Kraken (15:00+)
- Replaces Gold Mine
- HP: 4000 (neutral), 3600 (captured)
- Damage: 330 (neutral), 1000 (captured)
- Armor/Shield: 200
- XP: 250
- When captured, pushes down lane attacking turrets

### Minion Mines (4:00)
- Two mines, one on each side
- Capturing increases your lane minion strength
- Stronger minions give more gold/XP to enemies

## Hero Stats (Ringo as Reference)

### Base Stats (Level 1 - Level 12)
- Health: 703 - 2107
- Energy: 163 - 416
- Armor: 20 - 50
- Shield: 20 - 50
- Weapon Power: 71 - 130
- Attack Speed: 100% - 136.3%
- Attack Range: 6.2
- Move Speed: 3.1

### Per Level Gains
- +69 Health
- +22 Energy
- +6 Armor
- +6 Shield
- +3% Attack Speed
- +6 Weapon Power

## Ringo's Abilities

### Passive: Double Down
Next basic attack after killing ANYTHING is a critical strike.

### A - Achilles Shot
| Level | Cooldown | Energy | Damage | Slow | Duration |
|-------|----------|--------|--------|------|----------|
| 1 | 9s | 40 | 80 | 30% | 1.5s |
| 2 | 8.5s | 50 | 125 | 35% | 1.5s |
| 3 | 8s | 60 | 170 | 40% | 1.5s |
| 4 | 7.5s | 70 | 215 | 45% | 1.5s |
| 5 | 7s | 100 | 350 | 50% | 2.5s |

### B - Twirling Silver
| Level | Cooldown | Energy | Attack Speed | Move Speed | Duration |
|-------|----------|--------|--------------|------------|----------|
| 1 | 11s | 30 | +30% | +0.75 | 6s |
| 2 | 11s | 35 | +40% | +0.80 | 6s |
| 3 | 11s | 40 | +50% | +0.85 | 6s |
| 4 | 11s | 45 | +60% | +0.90 | 6s |
| 5 | 11s | 50 | +80% | +1.00 | 6s |

Special: Basic attacks reduce cooldown by 0.6s

### Ultimate - Hellfire Brew
| Level | Cooldown | Energy | Damage | Burn/sec | Burn Duration |
|-------|----------|--------|--------|----------|---------------|
| 1 | 100s | 100 | 250 | 30 | 7s |
| 2 | 85s | 115 | 365 | 50 | 7s |
| 3 | 70s | 130 | 480 | 70 | 7s |

- 100% Shield Pierce
- Global range (locks on target)
- Splash damage on impact

## Gold Economy

### Passive Income
- 1 gold per second

### Kill Rewards (Approximate)
| Source | Gold |
|--------|------|
| Lane Minion (melee) | 22 |
| Lane Minion (ranged) | 17 |
| Jungle small | 22-32 |
| Jungle big | 64-75 |
| Hero kill | 150-400 (based on level/bounty) |
| Turret | 100 |
| Assist | 50% of kill gold |

### Items
- Build from components
- Six item slots
- Core stats: Weapon Power, Crystal Power, Attack Speed, Armor, Shield, Health, Cooldown Reduction

## Respawn Times
- Early game: ~10 seconds
- Mid game: ~20-30 seconds
- Late game: 30-60+ seconds
- Scales with level and game time

## Controls (Touch)

- **Tap ground**: Move to location
- **Tap enemy**: Attack (auto-attacks until stopped)
- **Tap ability button**: Activate ability
- **Drag from ability**: Aim skillshot
- **Tap + hold**: Show range indicators
- **Home button**: Teleport to base (5s channel)

## Game Flow

### Early Game (0:00 - 7:00)
- Focus on last hitting minions
- Jungle farm
- Avoid deaths
- First items

### Mid Game (7:00 - 15:00)
- Team fights begin
- Contesting objectives (Gold Mine, Minion Mines)
- Pushing turrets
- Building core items

### Late Game (15:00+)
- Kraken spawns
- Full team fights
- One fight can decide game
- Long respawn timers
