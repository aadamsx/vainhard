# Ringo's Abilities - Vainglory

## Heroic Perk: Double Down
**Type:** Passive
**Effect:** Killing an enemy (minion, hero, jungle monster) causes Ringo's next basic attack to critically strike.
**Implementation:**
- Listen for kill events on any enemy
- Set a flag `nextAttackCrits = true`
- On next basic attack, apply critical damage (2x or configured multiplier)
- Clear the flag after the attack

## A Ability: Achilles Shot
**Type:** Skillshot (line projectile)
**Effect:** Fires a bullet in a direction that damages the first enemy hit and slows them.
**Stats (by level 1-5):**
- Damage: 80 / 125 / 170 / 215 / 350 (+125% Crystal Power)
- Slow: 30% / 35% / 40% / 45% / 50%
- Slow Duration: 1.5s / 1.5s / 1.5s / 1.5s / 2.5s
- Cooldown: 9s / 8.5s / 8s / 7.5s / 7s
- Energy Cost: 40 / 50 / 60 / 70 / 100
- Range: ~10 units

**Implementation:**
1. Player clicks ability, then clicks/drags to aim direction
2. Fire a projectile in that direction
3. Projectile travels until it hits an enemy or reaches max range
4. On hit: Deal damage, apply slow debuff to enemy's movement speed
5. Slow debuff: Reduce `NavMeshAgent.speed` by slow%, restore after duration

## B Ability: Twirling Silver
**Type:** Instant self-buff
**Effect:** Ringo gains bonus attack speed. Each basic attack while active reduces ALL ability cooldowns.
**Stats (by level 1-5):**
- Attack Speed Bonus: 50% / 60% / 70% / 80% / 100%
- Duration: 6s
- Cooldown Reduction per Attack: 0.6s (applies to A, B, and C abilities)
- Cooldown: 12s / 11s / 10s / 9s / 8s
- Energy Cost: 40 / 45 / 50 / 55 / 60

**Implementation:**
1. Player clicks ability (instant cast, no targeting needed)
2. Apply attack speed buff to CharacterStats
3. Subscribe to hero's OnAttack event
4. Each attack while buff active: reduce cooldowns on all abilities by 0.6s
5. After duration expires: remove attack speed buff, unsubscribe from attack event
6. Visual: Could show particles or glow around hero (NOT orbiting balls)

## C Ability (Ultimate): Hellfire Brew
**Type:** Targeted (click on enemy)
**Effect:** Fires a homing fireball at target enemy that tracks them and explodes on contact.
**Stats (by level 1-3):**
- Damage: 250 / 365 / 480 (+140% Crystal Power, +60% Weapon Power)
- Burning: Additional 10% Crystal Power damage over time
- Cooldown: 60s / 50s / 40s
- Energy Cost: 100 / 115 / 130
- Range: ~12 units (to start, then fireball tracks)

**Implementation:**
1. Player clicks ability, then clicks on an enemy target
2. Validate target is an enemy (hero, minion, monster)
3. Fire a homing projectile that follows the target
4. Projectile tracks target until it hits them (cannot be dodged easily)
5. On hit: Deal burst damage + apply burning debuff
6. Burning debuff: Deal damage over time for X seconds
7. Visual: Flaming projectile with trail

---

## Current Problems Identified

### Twirling Silver Issues:
1. **Visual is wrong** - Shows orbiting balls instead of attack speed buff indicator
2. **Attack speed buff may not be applied** - Need to verify CharacterStats.attackSpeed is modified
3. **Cooldown reduction on attacks** - Need to verify this triggers per attack

### What needs to be checked:
1. Does TwirlingSilver.Activate() modify the hero's attack speed?
2. Does it subscribe to attack events to reduce cooldowns?
3. Does it have a duration timer to remove the buff?
4. Are the visual effects appropriate (not orbiting balls)?

---

## Flow for Each Ability

### Instant Ability (Twirling Silver):
```
User clicks B button
  -> AbilityBarUI.OnAbilityClicked(1)
  -> Check: IsReady && CanAfford
  -> Since Targeting == Instant: playerHero.UseAbility(1, null, null)
  -> HeroController.UseAbility() -> abilities[1].TryActivate()
  -> TwirlingSilver.TryActivate() -> Activate()
  -> Apply attack speed buff to CharacterStats
  -> Start duration timer
  -> Subscribe to attack events for cooldown reduction
```

### Skillshot (Achilles Shot):
```
User clicks A button
  -> AbilityBarUI.OnAbilityClicked(0)
  -> Since Targeting == Skillshot: GameHUD.StartAbilityTargeting(0, ability)
  -> Show aim indicator (line from hero)
  -> User clicks/drags to aim, releases to fire
  -> playerHero.UseAbility(0, targetPosition, null)
  -> AchillesShot.TryActivate() -> Activate()
  -> Fire projectile toward targetPosition
  -> Projectile hits enemy -> deal damage + apply slow
```

### Targeted (Hellfire Brew):
```
User clicks C button
  -> AbilityBarUI.OnAbilityClicked(2)
  -> Since Targeting == Targeted: GameHUD.StartAbilityTargeting(2, ability)
  -> Show valid targets highlighted
  -> User clicks on enemy
  -> playerHero.UseAbility(2, null, targetUnit)
  -> HellfireBrew.TryActivate() -> Activate()
  -> Fire homing projectile at targetUnit
  -> Projectile tracks until hit -> deal damage + burning
```
