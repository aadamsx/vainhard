# Vainglory MOBA - Mental Model

## What Is a MOBA?

A MOBA is a competitive strategy game where two teams fight to destroy the enemy's base. The core experience is:
- **You control ONE hero** in a world with many other entities
- **Your hero grows stronger** over time through gold and experience
- **You must work with your team** (minions, allies) to push toward the enemy base
- **The enemy is doing the same thing** - it's a race and a battle

---

## Core Resource Systems

Every hero manages three core resources that interact constantly:

### Health (HP)

**What it is:** Your life force. When it reaches zero, you die.

**Mechanics:**
- Every hero has a **maximum health** value
- **Current health** can never exceed maximum
- Health is displayed as a bar (green for allies, red for enemies)
- When health reaches 0, the hero dies

**What affects health:**
- **Taking damage** → reduces current health
- **Healing** → restores current health (up to max)
- **Regeneration** → slowly restores health over time
- **Items** → can increase max health, regen rate, or provide lifesteal
- **Leveling up** → increases max health

**Death consequences:**
- Hero becomes inactive (can't move, attack, use abilities)
- Hero respawns after a delay (longer at higher levels)
- Hero respawns at their base with full health/energy
- Killer gets gold reward
- Nearby allies get assist gold
- Enemy gains map control during your death timer

**The Point:** Health is the ultimate limiter. You can have full energy and abilities ready, but if you're low health, you're vulnerable. Managing health (when to fight, when to retreat) is core gameplay.

### Energy (Stamina/Mana)

**What it is:** Your ability fuel. Abilities cost energy to cast.

**Mechanics:**
- Every hero has a **maximum energy** value
- **Current energy** can never exceed maximum
- Energy regenerates passively over time
- Energy is displayed as a blue bar below health

**What affects energy:**
- **Using abilities** → reduces current energy by ability cost
- **Regeneration** → slowly restores energy over time (base rate + bonuses)
- **Items** → can increase max energy or regen rate
- **Leveling up** → may increase max energy
- **Returning to base** → fully restores energy

**The Point:** Energy gates ability spam. You can't just press buttons forever - each ability costs something. This creates:
- **Early game scarcity** - Low energy pool, must be selective about ability use
- **Choice tension** - Do I use ability now or save for later?
- **Sustain gameplay** - Heroes with better energy management can stay in lane longer

### The Health-Energy Interaction

These systems interact in critical ways:

**Scenario 1: Full health, no energy**
- You can't use abilities
- You can only basic attack
- You're weak in a fight (no burst damage, no CC, no escapes)
- Smart play: Farm passively, wait for energy to regen

**Scenario 2: Low health, full energy**
- You have all abilities ready
- But one mistake = death
- Smart play: Either burst enemy down quickly, or retreat

**Scenario 3: Low health, low energy**
- Most vulnerable state
- Can't fight, can't use abilities to escape
- Smart play: Retreat to base immediately

**Scenario 4: Full health, full energy**
- Peak combat readiness
- Can engage fights confidently
- Can use abilities freely
- Smart play: Look for opportunities to fight

---

## Cooldowns

**What it is:** The time lock on abilities after use.

**Mechanics:**
- When you use an ability, it goes "on cooldown"
- During cooldown, you CANNOT use that ability
- Cooldown counts down in real time
- When cooldown reaches 0, ability is "ready" again
- Cooldown is per-ability, not shared

**What affects cooldowns:**
- **Ability level** → usually reduces cooldown at higher levels
- **Items** → cooldown reduction (CDR) items reduce all cooldowns by a %
- **Abilities** → some abilities reduce other cooldowns (like Twirling Silver)

**The Point:** Cooldowns create combat pacing:
- Use ability → window of weakness → ability ready → window of strength
- Tracking enemy cooldowns is key (if they just used their escape, they're vulnerable)
- Cooldown reduction is powerful because it increases ability uptime

### The Ability Gating System

For an ability to be usable, ALL of these must be true:
1. ✅ Ability is NOT on cooldown
2. ✅ You have ENOUGH energy to pay the cost
3. ✅ You have a VALID target (if ability requires one)
4. ✅ You are NOT crowd controlled (stunned, silenced)

If ANY of these fail, the ability won't activate:
- On cooldown? Can't use. Wait.
- Not enough energy? Can't use. Wait for regen or return to base.
- No valid target? Can't use (for targeted abilities).
- Stunned? Can't use anything.

---

## Stats System

Every hero has stats that determine their combat effectiveness. Stats are numbers that affect gameplay calculations.

### Offensive Stats

**Weapon Power (WP)**
- Increases basic attack damage
- Scales some abilities (WP ratios)
- Primary stat for "weapon path" heroes
- Gained from items and levels

**Crystal Power (CP)**
- Scales ability damage (CP ratios)
- Primary stat for "crystal path" heroes
- Gained from items

**Attack Speed**
- How fast you attack (attacks per second)
- Base value around 1.0 (one attack per second)
- Percentage increases are multiplicative
- Example: 100% attack speed bonus = 2.0 attacks/second
- Capped to prevent broken gameplay

### Defensive Stats

**Armor**
- Reduces physical damage (basic attacks, WP abilities)
- Higher armor = less damage taken
- Formula: Reduction = Armor / (Armor + 100)
- 100 armor = 50% reduction
- 200 armor = 67% reduction (diminishing returns)

**Shield**
- Reduces crystal damage (CP abilities)
- Same formula as armor, but for magic damage
- 100 shield = 50% reduction

### Utility Stats

**Movement Speed**
- How fast you walk
- Base speed around 3-4 units/second
- Can be increased by items/abilities
- Can be decreased by slows

**Attack Range**
- How far you can basic attack from
- Melee heroes: ~2 units
- Ranged heroes: ~6 units
- Can't be increased (hero-specific)

### Stat Interactions

**Weapon Power + Attack Speed = DPS**
```
DPS = WeaponPower × AttackSpeed
Example: 100 WP × 1.5 AS = 150 damage per second
```

**Damage vs Defense**
```
Raw Damage = 100
Target Armor = 100
Reduction = 100 / (100 + 100) = 50%
Final Damage = 100 × (1 - 0.50) = 50
```

**The Point:** Stats create build choices. Do you stack more damage or get tankier? Do you want burst (WP/CP) or sustained DPS (Attack Speed)? Items let you customize your stats for different strategies.

---

## Damage System

### Damage Types

**Physical Damage**
- From basic attacks
- From abilities with WP scaling
- Reduced by target's Armor

**Crystal Damage**
- From abilities with CP scaling
- Reduced by target's Shield

**True Damage**
- Rare, special damage type
- NOT reduced by any defense
- Used for executes and special effects

### The Complete Damage Flow

When you attack an enemy, here's exactly what happens:

```
1. CALCULATE RAW DAMAGE
   Basic Attack: Raw = WeaponPower × DamageMultiplier
   Ability: Raw = BaseDamage + (Stat × Ratio)

2. APPLY CRIT (if applicable)
   If critical hit: Raw = Raw × CritMultiplier (usually 1.5-2.0)

3. GET TARGET DEFENSE
   Physical damage → use target's Armor
   Crystal damage → use target's Shield

4. CALCULATE REDUCTION
   Reduction = Defense / (Defense + 100)

5. APPLY REDUCTION
   Final = Raw × (1 - Reduction)

6. SUBTRACT FROM HEALTH
   Target.CurrentHealth = Target.CurrentHealth - Final

7. CHECK DEATH
   If Target.CurrentHealth <= 0:
      Target dies
      Killer gets rewards
```

### Damage Sources

**Basic Attacks:**
- Click on enemy → hero auto-attacks
- Each attack deals WeaponPower damage
- Attack frequency = AttackSpeed stat
- Can crit (with items that give crit chance)

**Abilities:**
- Press ability button → ability activates
- Damage = BaseDamage + (Scaling × Stat)
- Example: Achilles Shot = 80 + (0.7 × CrystalPower)
- Abilities have their own cooldowns

**Damage Over Time (DoT):**
- Applied by some abilities
- Deals damage every tick (usually every second)
- Burns, poisons, bleeds
- Stacks or refreshes depending on ability

**Area Damage:**
- Some abilities hit an area
- ALL enemies in area take damage
- Each enemy is damaged independently

### The Point of the Damage System

The damage system creates:
1. **Counterplay** - Build armor vs WP heroes, shield vs CP heroes
2. **Choices** - Stack damage or get tanky?
3. **Diminishing returns** - Can't become invincible with infinite armor
4. **Meaningful combat** - Fights have clear winners based on stats and play

---

## Buffs and Debuffs

Temporary stat modifications that change combat effectiveness.

### Buffs (Positive Effects)

**What they are:** Temporary improvements to YOUR stats.

**Examples:**
- Attack speed buff (+50% attack speed for 6 seconds)
- Movement speed buff (+20% speed for 3 seconds)
- Shield buff (absorbs 200 damage)
- Damage boost (+30% damage for next attack)

**Key Properties:**
- Has a **duration** (expires after X seconds)
- Has a **magnitude** (how much it changes the stat)
- Applied to **ONE hero only** (the one using the ability)
- Displayed as an icon on your UI

**How Buffs Work:**
```
1. Ability activates (e.g., Twirling Silver)
2. Buff is applied to CASTER only
3. Stat is modified (e.g., attack speed × 1.5)
4. Timer starts counting down
5. When timer expires, stat returns to normal
6. Buff icon disappears
```

### Debuffs (Negative Effects)

**What they are:** Temporary impairments applied to ENEMIES.

**Examples:**
- Slow (-30% movement speed)
- Armor reduction (-20 armor)
- Damage reduction (deal 25% less damage)
- Mortal wound (healing reduced by 50%)

**How Debuffs Work:**
```
1. Ability hits enemy (e.g., Achilles Shot)
2. Debuff is applied to TARGET
3. Target's stat is reduced
4. Timer counts down
5. When timer expires, stat returns to normal
```

### Crowd Control (CC)

Special debuffs that impair action, not just stats.

**Slow:**
- Reduces movement speed
- Can still attack and use abilities
- Most common CC type

**Stun:**
- Cannot move
- Cannot attack
- Cannot use abilities
- Most powerful CC (usually short duration)

**Root/Snare:**
- Cannot move
- CAN still attack and use abilities
- Good for pinning down ranged heroes

**Silence:**
- Can move and attack
- CANNOT use abilities
- Counters ability-dependent heroes

**Blind:**
- Can move
- Basic attacks miss
- Counters attack-dependent heroes

### The Buff/Debuff Contract

**CRITICAL RULE:** Buffs affect the CASTER. Debuffs affect the TARGET.

```
I use Twirling Silver:
  → I get attack speed buff ✓
  → Enemy gets nothing ✗

I hit enemy with Achilles Shot:
  → Enemy gets slowed ✓
  → I get nothing from the slow ✗
```

**There is NEVER crossover:**
- My self-buff doesn't buff enemies
- My self-buff doesn't buff allies (unless ability says it does)
- Enemy's debuff on me doesn't affect my allies
- Everything is per-hero, per-effect

---

## The Economy System

### Gold

**What it is:** Currency for buying items.

**Sources of gold:**
- **Passive income** - Everyone gets gold over time (slowly)
- **Last-hitting minions** - Kill the minion yourself = gold (most important)
- **Killing heroes** - Large gold reward (300+ gold)
- **Assists** - Help kill a hero = smaller gold reward
- **Objectives** - Killing jungle bosses, gold mines

**Gold Flow:**
```
Farm minions → Get gold → Buy items → Get stronger → Win fights → Get more gold
```

**The Point:** Gold differentiates good players from bad. A player who last-hits well has more items, making them stronger in fights. Gold is the primary progression mechanic.

### Items

**What they are:** Permanent upgrades bought at the shop.

**Properties:**
- Cost gold
- Give stat bonuses (Weapon Power, Armor, etc.)
- Some have special effects (lifesteal, cooldown reduction)
- Limited inventory slots (usually 6)
- Can only buy at shop (in base)

**Item Tiers:**
- **Tier 1 (cheap):** Small bonuses, building blocks
- **Tier 2 (medium):** Moderate bonuses, combines Tier 1 items
- **Tier 3 (expensive):** Large bonuses, full items, often unique passives

**The Build Path:**
```
Start: 0 items
Early game: Tier 1 items (boots, small sword)
Mid game: Tier 2 items (bigger bonuses)
Late game: Tier 3 items (full build, 6 items)
```

**The Point:** Items let you customize your hero. Same hero, different items = different playstyle. Items also represent your gold advantage - more items = you're winning.

---

## Experience and Levels

### Experience (XP)

**What it is:** Progress toward leveling up.

**Sources:**
- **Nearby minion deaths** - You get XP if you're close when a minion dies
- **Hero kills/assists** - Large XP reward
- **Jungle monsters** - XP when you kill them

**Sharing:**
- XP is shared among nearby allied heroes
- More allies nearby = less XP each
- This is why "solo lane" gives more XP than duo lane

### Levels

**What levels do:**
- **Increase base stats** - More health, more damage
- **Unlock abilities** - Level 1, 2, 3 = unlock A, B, C abilities
- **Upgrade abilities** - More points in ability = stronger version

**Level Range:** 1 to 12

**Level Advantage:**
- 2 levels ahead = significant stat advantage
- Level matters most in early game
- By late game, everyone is near max level

### The XP-Gold Relationship

Both matter, but differently:

**XP:** Levels you up (automatic power from stats)
**Gold:** Lets you buy items (chosen power from items)

A hero with high level but no items has strong base stats.
A hero with lots of items but low level has strong item stats.
The ideal is both: high level AND good items.

---

## The World

### The Map
The map is a battlefield with two sides - yours and theirs. In the middle is contested territory.

```
THEIR BASE
   |
THEIR TERRITORY (dangerous for you)
   |
CONTESTED ZONE (where fights happen)
   |
YOUR TERRITORY (safe for you)
   |
YOUR BASE
```

**Your Base:**
- Where you spawn
- Where you respawn after dying
- Where you buy items
- Protected by YOUR turrets and YOUR crystal
- Safe zone - enemies take huge risks coming here

**Their Base:**
- Same as above, but for the enemy
- Your goal is to destroy it
- Dangerous for you - their turrets will kill you

**The Lane:**
- The path between bases
- Where minions walk
- Where most fighting happens
- Turrets guard key points along it

**The Jungle:**
- Side areas off the main lane
- Contains monsters that give gold/buffs when killed
- Good for gaining advantages without direct fighting

### Turrets
Turrets are defensive structures that:
- **Protect** the path to the base
- **Attack enemies** automatically (prioritizing minions, then heroes)
- **Must be destroyed in order** - outer first, then inner, then base
- **Hurt a LOT** - you can't just walk past them

The turret system creates a "front line" - you can only push as far as the turrets allow. To advance, you must destroy them. To destroy them, you need minions to tank the damage.

### The Crystal (Win Condition)
Each base has a crystal. Destroy the enemy crystal = you win. But:
- Crystal is protected by turrets
- You can't damage it until turrets are down
- It's deep in enemy territory

---

## Entities in the World

### Heroes (Players)
Every hero in the game is controlled by someone (human or AI). Each hero:

**Is completely independent:**
- Has their own health pool
- Has their own energy/mana pool
- Has their own gold
- Has their own experience and level
- Has their own inventory of items
- Has their own abilities with their own cooldowns
- Has their own position on the map
- Makes their own decisions

**No hero shares anything with another hero.** When I level up, only I level up. When I buy an item, only I get it. When I use an ability, only my ability goes on cooldown.

**Heroes interact through:**
- Dealing damage to each other
- Applying crowd control (stuns, slows)
- Healing allies (if they have healing abilities)
- Being in proximity (some abilities affect nearby allies/enemies)

### Minions
Minions are AI-controlled units that:
- Spawn periodically from each base
- Walk down the lane toward the enemy base
- Attack enemy minions, heroes, and turrets they encounter
- Provide gold and XP when killed

**Why minions matter:**
- They tank turret shots (so you can hit the turret)
- They provide gold (last-hitting is important)
- They push the lane (creating pressure)
- They provide XP (helps you level up)

Each team has their own minions. Your minions help you. Their minions help them.

### Jungle Monsters
Neutral creatures in the jungle that:
- Don't belong to either team
- Attack whoever attacks them
- Give gold/XP/buffs when killed
- Respawn after a delay

The jungle is a secondary source of income. Good players farm jungle between waves.

---

## Hero Progression

### Experience and Levels
- Heroes start at level 1
- Gain XP from nearby minion deaths, kills, and objectives
- Each level increases base stats (health, damage, etc.)
- Each level may unlock or upgrade abilities
- Max level is 12

### Gold and Items
- Heroes earn gold from minions, kills, passive income, objectives
- Gold is spent at the shop (in base) to buy items
- Items give permanent stat bonuses
- Better items = stronger hero
- Gold advantage = power advantage

### The Snowball Effect
If you're ahead (more gold, higher level), you're stronger. Being stronger means you win fights. Winning fights means you get more gold/XP. This is the "snowball" - small advantages compound.

---

## Abilities

Each hero has a kit of abilities (typically 3-4). Abilities are what make each hero unique.

### Ability Resources
- **Cooldown:** Time before you can use the ability again
- **Energy/Mana:** Resource consumed when casting
- Both must be available to use an ability

### Ability Types by Targeting

**Instant (Self-Cast):**
- Press button, effect happens immediately
- Usually buffs yourself
- Example: "Gain 50% attack speed for 6 seconds"
- Affects ONLY you

**Skillshot (Aim and Fire):**
- Press button, aim direction, release to fire
- Projectile travels in that direction
- Hits the first enemy in its path (usually)
- Requires prediction and aim
- Example: "Fire a bullet that damages and slows"

**Targeted (Click on Enemy):**
- Press button, click on valid target
- Ability affects that specific target
- Can't miss once cast
- Example: "Launch a homing fireball at target enemy"

**Area of Effect (AoE):**
- Press button, click on location
- Ability affects an area
- Example: "Call down fire on target area"

### Ability Effects

**Damage:** Reduces enemy health
**Crowd Control (CC):** Impairs enemy (slow, stun, silence, root)
**Buff:** Improves your stats temporarily
**Debuff:** Reduces enemy stats temporarily
**Heal:** Restores health to self or allies
**Shield:** Adds temporary health buffer
**Dash/Blink:** Moves your hero instantly

### The Complete Ability Activation Flow

When you press an ability button, here's EXACTLY what happens:

```
STEP 1: CHECK IF ABILITY CAN BE USED
├── Is ability on cooldown? → YES: Show "not ready" feedback, STOP
├── Do I have enough energy? → NO: Show "not enough energy" feedback, STOP
├── Am I stunned/silenced? → YES: Nothing happens, STOP
└── All checks pass → CONTINUE

STEP 2: HANDLE TARGETING (depends on ability type)
├── Instant: No targeting needed → GO TO STEP 3
├── Skillshot: Show aim indicator, wait for release/confirm → GO TO STEP 3
├── Unit Target: Wait for target click → GO TO STEP 3
└── Point Target: Wait for ground click → GO TO STEP 3

STEP 3: CONSUME RESOURCES
├── Subtract energy cost from MY current energy
├── Put ability on cooldown (MY cooldown, no one else's)
└── CONTINUE

STEP 4: EXECUTE EFFECT (depends on ability type)
├── Instant self-buff:
│   └── Apply buff to ME only
│
├── Skillshot:
│   ├── Spawn projectile at MY position
│   ├── Projectile travels in aimed direction
│   ├── On hit: Apply damage/debuff to HIT ENEMY only
│   └── Miss: Nothing happens to anyone
│
├── Unit Target:
│   └── Apply effect to CLICKED TARGET only
│
└── Point Target:
    ├── Create effect at CLICKED LOCATION
    └── Affect ALL ENEMIES in area (independently)

STEP 5: VISUAL FEEDBACK
├── Play animation on MY hero
├── Play particle effects
├── Update UI (cooldown timer, energy bar)
└── If debuff applied: Show debuff indicator on TARGET
```

### Why This Flow Matters

Every ability follows this exact pattern. Understanding it prevents bugs:

**Bug Example: "Enemy gets my buff"**
- This means Step 4 applied the buff to wrong target
- Self-buffs should ONLY modify the caster's stats
- The ability's owner is the ability's target for self-buffs

**Bug Example: "Ability used but no energy spent"**
- Step 3 failed - resource consumption must happen
- Energy subtraction must affect the caster only

**Bug Example: "Used ability while stunned"**
- Step 1 check was missing or wrong
- CC checks must prevent ability activation

### The Golden Rule of Abilities

**Your abilities affect what they're designed to affect, nothing more.**

- Self-buff → affects only you
- Damage skillshot → affects only enemy it hits
- Targeted ability → affects only the target

You NEVER accidentally affect other heroes. The game is deterministic - ability X does effect Y to target Z, period.

---

## Combat

### Basic Attacks
Every hero can basic attack (auto-attack):
- Click on enemy to start attacking
- Hero automatically attacks at their attack speed
- Deals physical damage based on Weapon Power
- Reduced by target's Armor

### Ability Damage
Abilities deal damage based on:
- Base damage (from ability level)
- Scaling (usually from Weapon Power or Crystal Power)
- Reduced by target's Armor (physical) or Shield (crystal)

### The Damage Formula
```
Raw Damage = Base + (Your Stats × Ratios)
Reduction = Target's Defense / (Target's Defense + 100)
Final Damage = Raw Damage × (1 - Reduction)
```

### Kill Credit
When an enemy dies:
- The killer (last hit) gets gold
- Nearby allies get assist gold (less)
- XP is shared among nearby allies

**This matters for abilities like Ringo's Double Down** - only triggers when YOU get the kill.

---

## The Two Teams

### Your Team (Blue)
- Your hero
- Your minions
- Your turrets
- Your crystal
- (In 3v3: Your two allies)

### Enemy Team (Red)
- Enemy heroes
- Enemy minions
- Enemy turrets
- Enemy crystal

### Neutral
- Jungle monsters (until attacked)

### Team Interactions

| You | Your Team | Enemies | Neutral |
|-----|-----------|---------|---------|
| Can't damage | Can heal/buff (if ability allows) | Can damage | Can damage |
| Can't be damaged by | Protected together | Damaged by | Damaged by (if you aggro) |

---

## Game Flow

### Early Game (0-5 minutes)
- Focus on farming minions for gold
- Avoid unnecessary fights
- Poke when safe, but don't overcommit
- Jungle when lane is pushed

### Mid Game (5-15 minutes)
- First turrets start falling
- Team fights become important
- Objectives matter more (gold mines, jungle bosses)
- Item power spikes happen

### Late Game (15+ minutes)
- Full builds, max levels
- One team fight can end the game
- Pushing with minions is critical
- Death timers are long (30-60 seconds)

### The Push Pattern
To destroy the enemy base:
1. Build a minion wave (don't kill enemy minions, let yours stack)
2. Push with the wave to enemy turret
3. Minions tank turret, you damage turret
4. Destroy outer turret
5. Repeat for inner turret
6. Repeat for base turret
7. Destroy crystal, win

---

## Ringo Specifically

### Who is Ringo?
A ranged carry. "Carry" means he deals high damage but is fragile. "Ranged" means he attacks from a distance. His job is to deal damage while staying safe.

### Heroic Perk: Double Down
**Concept:** Reward for last-hitting. Kill something → next attack crits.

**Why it exists:** Encourages good play (last-hitting minions). Rewards precision. Creates moments of burst damage.

**What happens:**
1. Ringo kills any enemy (minion, hero, monster)
2. A flag is set: `nextAttackCrits = true`
3. Ringo's next basic attack deals 1.8x damage
4. Flag is cleared

**What DOESN'T happen:**
- Doesn't affect any other hero
- Doesn't trigger if an ally gets the kill
- Doesn't stack (kill 5 minions = still just 1 crit)

### A Ability: Achilles Shot
**Concept:** Poke and slow. Good for harassing and chasing.

**What happens:**
1. Ringo presses A, aims direction
2. A projectile fires in that direction
3. Projectile hits first enemy in path
4. That enemy takes damage AND is slowed

**What it's for:**
- Harassing enemy heroes in lane
- Slowing enemies so you can escape
- Slowing enemies so you can chase
- Finishing low-health enemies

### B Ability: Twirling Silver
**Concept:** Steroid for DPS. Ringo becomes an attack speed monster.

**What happens:**
1. Ringo presses B
2. ONLY Ringo gets +50-100% attack speed for 6 seconds
3. ONLY Ringo shows the visual effect
4. Each basic attack Ringo lands reduces ALL his cooldowns by 0.6s

**What it's for:**
- Melting objectives (turrets, crystal)
- Winning 1v1 fights through raw DPS
- Resetting abilities faster in extended fights

**What DOESN'T happen:**
- Enemy heroes don't get attack speed
- Ally heroes don't get attack speed
- No one else's cooldowns are reduced

### C Ability: Hellfire Brew (Ultimate)
**Concept:** Guaranteed damage. You can't miss this.

**What happens:**
1. Ringo presses C, clicks on enemy hero
2. A fireball spawns and HOMES toward that target
3. Fireball chases until it hits
4. Target takes burst damage + burn over time

**What it's for:**
- Finishing kills (can't juke it)
- Starting fights with guaranteed damage
- Punishing enemies who overextend

---

## Interactions Visualized

### When I Use Twirling Silver:
```
ME: Press B
↓
MY ABILITY: Goes on cooldown (12s)
MY ENERGY: Reduced by cost (40)
MY STATS: Attack speed buffed (+50%)
MY HERO: Shows visual effect (circle)
↓
ENEMY: Nothing. Absolutely nothing.
ALLY: Nothing. Absolutely nothing.
```

### When Enemy Uses Twirling Silver:
```
ENEMY: Presses B (or AI decides to)
↓
ENEMY'S ABILITY: Goes on cooldown
ENEMY'S ENERGY: Reduced
ENEMY'S STATS: Attack speed buffed
ENEMY'S HERO: Shows visual effect
↓
ME: Nothing. I don't even know they used it unless I see the visual.
```

### When I Shoot Achilles Shot and Hit Enemy:
```
ME: Press A, aim, release
↓
MY ABILITY: Goes on cooldown
MY ENERGY: Reduced
PROJECTILE: Spawns, flies in direction
↓
PROJECTILE HITS ENEMY:
  ENEMY: Takes damage, is slowed
↓
ME: Nothing further
OTHER ENEMIES: Nothing
ALLIES: Nothing
```

---

## Key Principles

### 1. Independence
Every hero is isolated. Your button presses affect your hero. Their button presses affect their hero. The only crossover is through designed interactions (damage, CC, heals).

### 2. Ownership
Everything belongs to someone:
- My gold is MY gold
- My cooldowns are MY cooldowns
- My buffs are MY buffs
- My minions are MY team's minions

### 3. Intentional Interaction
Heroes only affect each other through explicit game mechanics:
- Damage (I shoot you, you lose health)
- CC (I slow you, you move slower)
- Heals (I heal ally, ally gains health)

There is no "leak" - my self-buff doesn't accidentally buff you.

### 4. Determinism
Given the same inputs, the same outputs happen. Ability X always does Y. There's no randomness in whether an ability affects the right target.

### 5. Multiplayer Mindset
Always design as if 6 human players are in the game. Each one is at their keyboard, pressing their own buttons, controlling their own hero. What one player does has no effect on what another player CAN do (only on what happens TO them if targeted).

---

## Questions Before Implementing Anything

1. **Whose hero does this affect?** (mine, enemy's, ally's, multiple?)
2. **Is this a self-effect or a targeted effect?**
3. **If I use this ability, what changes for EACH entity in the game?**
4. **If the enemy uses this same ability, what changes for me?**
5. **In a 6-player game, does this still make sense?**
6. **Am I accidentally crossing hero boundaries?**
7. **What is the POINT of this feature in gameplay terms?**

---

## System Interactions Summary

### How Everything Connects

```
┌─────────────────────────────────────────────────────────────────┐
│                        GAME STATE                                │
├─────────────────────────────────────────────────────────────────┤
│  HERO 1 (Player)              │  HERO 2 (Enemy)                 │
│  ├─ Health: 500/500           │  ├─ Health: 400/500             │
│  ├─ Energy: 200/300           │  ├─ Energy: 280/300             │
│  ├─ Level: 5                  │  ├─ Level: 4                    │
│  ├─ Gold: 1500                │  ├─ Gold: 1200                  │
│  ├─ Items: [Sword, Boots]     │  ├─ Items: [Shield]             │
│  ├─ Stats:                    │  ├─ Stats:                      │
│  │   WP: 80, CP: 0            │  │   WP: 60, CP: 40              │
│  │   Armor: 30, Shield: 20    │  │   Armor: 50, Shield: 30       │
│  │   AttackSpeed: 1.2         │  │   AttackSpeed: 1.0            │
│  ├─ Abilities:                │  ├─ Abilities:                  │
│  │   A: Ready (CD: 0/8s)      │  │   A: On CD (CD: 3/8s)         │
│  │   B: Active! (Buff on)     │  │   B: Ready (CD: 0/12s)        │
│  │   C: Ready (CD: 0/60s)     │  │   C: On CD (CD: 45/60s)       │
│  └─ Buffs: [Twirling Silver]  │  └─ Buffs: []                   │
│                               │                                  │
│  COMPLETELY INDEPENDENT       │  COMPLETELY INDEPENDENT          │
│  No shared state              │  No shared state                 │
└─────────────────────────────────────────────────────────────────┘
```

### The Input-to-Output Chain

```
USER INPUT                GAME LOGIC                    VISUAL OUTPUT
─────────────────────────────────────────────────────────────────────

Tap ground         →  Calculate path           →  Hero moves
                      Motor.MoveTo()               Animation plays
                      NavMesh pathfinding          Position updates

Tap enemy          →  Set attack target        →  Hero moves to range
                      Check range                  Attack animation
                      If in range: Attack          Projectile spawns
                      Deal damage                  Health bar decreases
                      Check death                  (Death animation)

Press A ability    →  Check can cast           →  Aim indicator shows
                      Wait for aim
                      On release:
                      Subtract energy          →  Energy bar decreases
                      Start cooldown           →  Button shows cooldown
                      Spawn projectile         →  Projectile flies
                      On hit: Damage + Slow    →  Enemy health drops
                                                  Slow icon appears

Press B ability    →  Check can cast
(Twirling Silver)     Subtract energy          →  Energy bar decreases
                      Start cooldown           →  Button shows cooldown
                      Apply buff to SELF ONLY  →  MY attack speed up
                      Spawn visual on SELF     →  Ground ring on ME
                      (nothing else changes)      (enemy unchanged)
```

### What Happens When Combat Occurs

```
SCENARIO: I attack enemy hero

1. MY ACTION
   └─ I click on enemy hero to attack

2. MY HERO'S STATE CHANGES
   ├─ currentTarget = enemy hero
   ├─ isAttacking = true
   └─ Start moving toward target

3. IN RANGE?
   ├─ NO: Keep moving toward target
   └─ YES: Stop moving, face target

4. ATTACK READY? (based on MY attack speed)
   ├─ NO: Wait for attack cooldown
   └─ YES: Perform attack

5. CALCULATE MY DAMAGE
   ├─ damage = MY WeaponPower × MY DamageMultiplier
   ├─ Check MY crit passive (Double Down)
   └─ Final damage value calculated

6. APPLY TO TARGET
   ├─ Get TARGET's armor
   ├─ Calculate reduction
   ├─ Subtract final damage from TARGET's health
   └─ Check if TARGET dies

7. IF TARGET DIES
   ├─ I get kill gold (added to MY gold)
   ├─ I get kill XP (added to MY XP)
   ├─ MY Double Down passive triggers (MY next attack crits)
   └─ TARGET respawns after delay

8. VISUAL FEEDBACK
   ├─ Attack projectile spawns from ME to TARGET
   ├─ TARGET's health bar updates
   ├─ Damage numbers appear
   └─ (If kill: Death animation, kill notification)
```

### Complete Game Loop

```
EVERY FRAME:
├─ Process Player Input
│   ├─ Movement commands → Hero.MoveTo()
│   ├─ Attack commands → Hero.SetAttackTarget()
│   └─ Ability commands → Ability.TryActivate()
│
├─ Update Each Hero (independently)
│   ├─ Update position (if moving)
│   ├─ Update attack (if targeting)
│   ├─ Update cooldowns (count down)
│   ├─ Update buffs (check expiration)
│   ├─ Update energy regen (passive gain)
│   └─ Update health regen (passive gain)
│
├─ Update Each Minion (independently)
│   ├─ Move toward enemy base
│   ├─ Attack nearest enemy in range
│   └─ Die if health <= 0
│
├─ Update Each Turret (independently)
│   ├─ Find target (prioritize minions, then heroes)
│   └─ Attack target in range
│
├─ Update Projectiles
│   ├─ Move toward target/direction
│   ├─ Check for hits
│   └─ Apply effects on hit
│
├─ Check Win Condition
│   ├─ Blue crystal destroyed? → Red wins
│   └─ Red crystal destroyed? → Blue wins
│
└─ Update UI
    ├─ Health/energy bars
    ├─ Cooldown indicators
    ├─ Buff icons
    ├─ Gold display
    └─ Level display
```

---

## Implementation Checklist

When implementing ANY feature, verify:

### For Abilities:
- [ ] Does the ability only affect intended targets?
- [ ] Does energy cost only deduct from the caster?
- [ ] Does cooldown only start on the caster?
- [ ] Do visual effects only spawn on/around appropriate targets?
- [ ] Is the owner reference correct throughout?

### For Buffs/Debuffs:
- [ ] Is the buff applied to the caster only (for self-buffs)?
- [ ] Is the debuff applied to the target only (for enemy-targeting)?
- [ ] Does the buff expire correctly?
- [ ] Is the stat modification temporary?

### For Damage:
- [ ] Is damage calculated from the ATTACKER's stats?
- [ ] Is damage reduced by the TARGET's defenses?
- [ ] Is the correct damage type used (physical/crystal)?
- [ ] Is kill credit given to the correct entity?

### For UI:
- [ ] Does the UI show THIS hero's state?
- [ ] Are ability buttons linked to THIS hero's abilities?
- [ ] Does the health bar show THIS hero's health?
- [ ] Does clicking an ability use THIS hero's ability?

### For AI:
- [ ] Does the AI control ITS hero independently?
- [ ] Do AI ability uses only affect AI's hero (for self-buffs)?
- [ ] Are AI decisions based on AI hero's state?
- [ ] Is the AI's hero completely separate from player's hero?
