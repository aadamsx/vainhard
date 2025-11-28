using UnityEngine;
using VaingloryMoba.Core;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Characters
{
    /// <summary>
    /// Main controller for player-controlled heroes.
    /// Connects input to movement, abilities, and attacks.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(CharacterStats))]
    public class HeroController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool isPlayerControlled = false;
        [SerializeField] private GameManager.Team team = GameManager.Team.Blue;

        [Header("Combat")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private GameObject attackProjectilePrefab;

        [Header("Abilities")]
        [SerializeField] private AbilityBase[] abilities = new AbilityBase[4];

        // Components
        private CharacterMotor motor;
        private CharacterStats stats;
        private Inventory inventory;

        // State
        private float lastAttackTime;
        private GameObject currentTarget;
        private bool isAttacking;

        // Properties
        public GameManager.Team Team => team;
        public CharacterStats Stats => stats;
        public CharacterMotor Motor => motor;
        public bool IsPlayerControlled => isPlayerControlled;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            stats = GetComponent<CharacterStats>();
            inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            // Get abilities from children (must be in Start, not Awake,
            // because abilities are added after HeroController in GameSceneSetup)
            abilities = GetComponentsInChildren<AbilityBase>();
            Debug.Log($"HeroController: Found {abilities.Length} abilities on {gameObject.name}");

            if (isPlayerControlled)
            {
                SubscribeToInput();
            }

            // Subscribe to death events
            stats.OnDeath.AddListener(OnDeath);
            stats.OnDeathWithKiller.AddListener(OnDeathWithKiller);

            // Subscribe to kill events for Double Down passive
            SubscribeToKillEvents();
        }

        private void SubscribeToKillEvents()
        {
            // Find all Minions and subscribe to their death events
            // Note: Dynamic minions will need to be subscribed when spawned
        }

        /// <summary>
        /// Called when this hero kills a target.
        /// </summary>
        public void OnKilledTarget(GameObject victim)
        {
            Debug.Log($"{gameObject.name} killed {victim.name}!");

            // Trigger Double Down passive for Ringo
            var ringo = GetComponent<RingoHero>();
            if (ringo != null)
            {
                ringo.OnKill(victim);
            }
        }

        private void OnDestroy()
        {
            if (isPlayerControlled)
            {
                UnsubscribeFromInput();
            }
        }

        private void SubscribeToInput()
        {
            if (TouchInputManager.Instance == null)
            {
                Debug.LogWarning("HeroController: TouchInputManager.Instance is null, retrying in 0.1s");
                Invoke(nameof(SubscribeToInput), 0.1f);
                return;
            }

            Debug.Log("HeroController: Subscribed to input events");
            TouchInputManager.Instance.OnMoveCommand.AddListener(OnMoveCommand);
            TouchInputManager.Instance.OnMoveHold.AddListener(OnMoveHold);
            TouchInputManager.Instance.OnUnitTapped.AddListener(OnUnitTapped);
        }

        private void UnsubscribeFromInput()
        {
            if (TouchInputManager.Instance == null) return;

            TouchInputManager.Instance.OnMoveCommand.RemoveListener(OnMoveCommand);
            TouchInputManager.Instance.OnMoveHold.RemoveListener(OnMoveHold);
            TouchInputManager.Instance.OnUnitTapped.RemoveListener(OnUnitTapped);
        }

        private void Update()
        {
            if (!stats.IsAlive) return;

            UpdateAttacking();
        }

        private void UpdateAttacking()
        {
            if (currentTarget == null)
            {
                isAttacking = false;
                return;
            }

            // Check if target is still valid
            var targetStats = currentTarget.GetComponent<CharacterStats>();
            if (targetStats == null || !targetStats.IsAlive)
            {
                currentTarget = null;
                isAttacking = false;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            // Move into range if needed
            if (distanceToTarget > stats.AttackRange)
            {
                // Use MoveTo instead of FollowTarget for reliable movement
                motor.MoveTo(currentTarget.transform.position);
            }
            else
            {
                motor.StopMoving();

                // Face target
                Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                // Attack if cooldown ready (include item attack speed)
                float effectiveAttackSpeed = stats.AttackSpeed;
                if (inventory != null)
                {
                    effectiveAttackSpeed *= (1f + inventory.BonusAttackSpeed);
                }
                if (Time.time >= lastAttackTime + (1f / effectiveAttackSpeed))
                {
                    PerformAttack();
                }
            }
        }

        private void PerformAttack()
        {
            if (currentTarget == null) return;

            // Get effective attack speed (base + items)
            float effectiveAttackSpeed = stats.AttackSpeed;
            if (inventory != null)
            {
                effectiveAttackSpeed *= (1f + inventory.BonusAttackSpeed);
            }

            Debug.Log($"PerformAttack: AttackSpeed={effectiveAttackSpeed:F2}, interval={1f/effectiveAttackSpeed:F2}s");
            lastAttackTime = Time.time;

            // Calculate base damage (including item weapon power)
            float damage = stats.WeaponPower * stats.DamageMultiplier;
            if (inventory != null)
            {
                damage += inventory.BonusWeaponPower * stats.DamageMultiplier;
            }

            // Check for Double Down passive (Ringo)
            bool isCrit = false;
            var ringo = GetComponent<RingoHero>();
            if (ringo != null)
            {
                float originalDamage = damage;
                damage = ringo.ModifyAttackDamage(damage);
                isCrit = damage > originalDamage; // Double Down triggered a crit
            }

            // Check for item crit chance (only if not already critting from passive)
            if (!isCrit && inventory != null && inventory.BonusCritChance > 0)
            {
                if (Random.value < inventory.BonusCritChance)
                {
                    // Base crit is 150% damage, plus any bonus crit damage from items
                    float critMultiplier = 1.5f + inventory.BonusCritDamage;
                    damage *= critMultiplier;
                    isCrit = true;
                    Debug.Log($"CRITICAL HIT! {critMultiplier * 100}% damage");
                }
            }

            // Get armor pierce values from inventory
            float armorPierce = 0f;
            float armorPiercePercent = 0f;
            if (inventory != null)
            {
                armorPierce = inventory.BonusArmorPierce;
                armorPiercePercent = inventory.BonusArmorPiercePercent;
            }

            // Apply damage to target with pierce
            var targetStats = currentTarget.GetComponent<CharacterStats>();
            float damageDealt = 0f;
            if (targetStats != null)
            {
                float healthBefore = targetStats.CurrentHealth;
                targetStats.TakeDamageWithPierce(damage, CharacterStats.DamageType.Physical, gameObject, armorPierce, armorPiercePercent);
                damageDealt = healthBefore - targetStats.CurrentHealth;
            }

            // Apply lifesteal (heal based on damage dealt)
            if (inventory != null && inventory.BonusLifesteal > 0 && damageDealt > 0)
            {
                float healAmount = damageDealt * inventory.BonusLifesteal;
                stats.Heal(healAmount);
                Debug.Log($"Lifesteal: Healed {healAmount:F1} HP ({inventory.BonusLifesteal * 100}% of {damageDealt:F1} damage)");
            }

            // Visual feedback
            CreateAttackEffect(isCrit);

            // Trigger attack event for abilities (Twirling Silver cooldown reduction)
            OnAttackHit();
        }

        private void CreateAttackEffect(bool isCrit = false)
        {
            if (currentTarget == null) return;

            // Create a visible projectile that flies to the target
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = isCrit ? "CritProjectile" : "AttackProjectile";
            projectile.transform.position = transform.position + Vector3.up * 1.2f;
            projectile.transform.localScale = isCrit ? Vector3.one * 0.45f : Vector3.one * 0.3f; // Bigger for crit

            // Remove collider so it doesn't interfere
            var col = projectile.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Color it bright yellow/orange, or red for crit
            var renderer = projectile.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = isCrit ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.8f, 0.2f);

            // Add the projectile mover component
            var mover = projectile.AddComponent<ProjectileMover>();
            mover.Initialize(currentTarget.transform.position + Vector3.up, 25f);

            Debug.Log($"Attack projectile fired at {currentTarget.name}" + (isCrit ? " (CRIT!)" : ""));
        }

        private void OnAttackHit()
        {
            // Notify abilities that an attack hit (for Twirling Silver)
            foreach (var ability in abilities)
            {
                ability?.OnBasicAttackHit();
            }
        }

        #region Input Handlers

        private void OnMoveCommand(Vector3 position)
        {
            Debug.Log($"HeroController.OnMoveCommand: {position}");
            if (TouchInputManager.Instance.IsAbilityTargeting) return;

            currentTarget = null;
            motor.MoveTo(position);
        }

        private void OnMoveHold(Vector3 position)
        {
            if (TouchInputManager.Instance.IsAbilityTargeting) return;

            if (currentTarget == null)
            {
                motor.MoveTo(position);
            }
        }

        private void OnUnitTapped(GameObject unit)
        {
            Debug.Log($"HeroController.OnUnitTapped: {unit.name}, my team: {team}");

            if (TouchInputManager.Instance.IsAbilityTargeting) return;

            // Check if it's an enemy hero
            var heroController = unit.GetComponent<HeroController>();
            if (heroController != null)
            {
                Debug.Log($"  Target is hero, their team: {heroController.Team}");
                if (heroController.Team != team)
                {
                    SetAttackTarget(unit);
                    return;
                }
            }

            // Check for other attackable units (minions, turrets, jungle)
            var targetable = unit.GetComponent<Targetable>();
            if (targetable != null)
            {
                Debug.Log($"  Target has Targetable, their team: {targetable.Team}, IsEnemyOf({team}): {targetable.IsEnemyOf(team)}");
                if (targetable.IsEnemyOf(team))
                {
                    SetAttackTarget(unit);
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetAttackTarget(GameObject target)
        {
            Debug.Log($"HeroController: Attack target set to {target.name}");
            currentTarget = target;
            isAttacking = true;
        }

        public void ClearTarget()
        {
            currentTarget = null;
            isAttacking = false;
        }

        public bool UseAbility(int index, Vector3? targetPosition = null, GameObject targetUnit = null)
        {
            if (index < 0 || index >= abilities.Length || abilities[index] == null)
                return false;

            return abilities[index].TryActivate(targetPosition, targetUnit);
        }

        public AbilityBase GetAbility(int index)
        {
            if (index < 0 || index >= abilities.Length)
                return null;
            return abilities[index];
        }

        public void Respawn(Vector3 position)
        {
            transform.position = position;
            stats.FullHeal();
            gameObject.SetActive(true);
        }

        public void SetAsPlayer(bool isPlayer)
        {
            if (isPlayerControlled && !isPlayer)
            {
                UnsubscribeFromInput();
            }
            else if (!isPlayerControlled && isPlayer)
            {
                SubscribeToInput();
            }
            isPlayerControlled = isPlayer;
        }

        public void SetTeam(GameManager.Team newTeam)
        {
            team = newTeam;
        }

        #endregion

        private void OnDeath()
        {
            motor.StopMoving();
            currentTarget = null;
            isAttacking = false;

            Debug.Log($"{gameObject.name} has died!");

            // Disable the hero
            gameObject.SetActive(false);
        }

        private void OnDeathWithKiller(GameObject killer)
        {
            // Notify the killer's HeroController (for Double Down passive)
            if (killer != null)
            {
                var killerHero = killer.GetComponent<HeroController>();
                if (killerHero != null)
                {
                    killerHero.OnKilledTarget(gameObject);
                }

                // Give killer gold and XP for hero kill
                var killerStats = killer.GetComponent<CharacterStats>();
                if (killerStats != null)
                {
                    killerStats.AddGold(300); // Hero kill gold (Vainglory value)
                    killerStats.AddExperience(200f); // Hero kill XP
                    Debug.Log($"{killer.name} killed hero {gameObject.name} for 300 gold!");
                }
            }
        }
    }
}
