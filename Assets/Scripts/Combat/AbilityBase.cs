using UnityEngine;
using UnityEngine.Events;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Base class for all abilities. Handles cooldowns, energy costs, and targeting.
    /// </summary>
    public abstract class AbilityBase : MonoBehaviour
    {
        [Header("Ability Info")]
        [SerializeField] protected string abilityName;
        [SerializeField] protected int abilityIndex; // 0=A, 1=B, 2=C, 3=Ultimate
        [SerializeField] protected Sprite icon;
        [TextArea]
        [SerializeField] protected string description;

        [Header("Costs & Cooldown")]
        [SerializeField] protected float energyCost = 40f;
        [SerializeField] protected float cooldown = 8f;
        [SerializeField] protected float[] cooldownPerLevel;

        [Header("Targeting")]
        [SerializeField] protected TargetingType targetingType = TargetingType.Skillshot;
        [SerializeField] protected float range = 8f;
        [SerializeField] protected float radius = 1f; // For AoE abilities

        [Header("Damage")]
        [SerializeField] protected float baseDamage = 80f;
        [SerializeField] protected float[] damagePerLevel;
        [SerializeField] protected float weaponRatio = 0f;
        [SerializeField] protected float crystalRatio = 1f;
        [SerializeField] protected CharacterStats.DamageType damageType = CharacterStats.DamageType.Crystal;

        // Events
        public UnityEvent OnAbilityActivated = new UnityEvent();
        public UnityEvent OnAbilityCooldownComplete = new UnityEvent();

        // State
        protected float lastUseTime = -100f;
        protected int currentLevel = 1;
        protected CharacterStats ownerStats;
        protected HeroController owner;
        protected Inventory ownerInventory;

        // Properties
        public string AbilityName => abilityName;
        public int AbilityIndex => abilityIndex;
        public Sprite Icon => icon;
        public string Description => description;
        public float EnergyCost => energyCost;
        public float Range => range;
        public float Radius => radius;
        public TargetingType Targeting => targetingType;

        /// <summary>
        /// Base cooldown without CDR applied
        /// </summary>
        public float BaseCooldown
        {
            get
            {
                if (cooldownPerLevel != null && currentLevel > 0 && currentLevel <= cooldownPerLevel.Length)
                    return cooldownPerLevel[currentLevel - 1];
                return cooldown;
            }
        }

        /// <summary>
        /// Effective cooldown with CDR from items applied
        /// </summary>
        public float Cooldown
        {
            get
            {
                float cd = BaseCooldown;
                // Apply cooldown reduction from inventory
                if (ownerInventory != null && ownerInventory.BonusCooldownReduction > 0)
                {
                    cd *= (1f - ownerInventory.BonusCooldownReduction);
                }
                return cd;
            }
        }

        public float CurrentCooldown
        {
            get
            {
                float remaining = (lastUseTime + Cooldown) - Time.time;
                return Mathf.Max(0, remaining);
            }
        }

        public float CooldownPercent
        {
            get
            {
                if (Cooldown <= 0) return 0;
                return CurrentCooldown / Cooldown;
            }
        }

        public bool IsReady => CurrentCooldown <= 0;
        public bool CanAfford => ownerStats != null && ownerStats.HasEnergy(energyCost);

        public enum TargetingType
        {
            Instant,        // No targeting needed (self-buff)
            Skillshot,      // Line from hero in aimed direction
            PointTarget,    // AoE at target location
            UnitTarget,     // Must target a unit
            Global          // Can target anywhere on map
        }

        protected virtual void Awake()
        {
            owner = GetComponentInParent<HeroController>();
            ownerStats = GetComponentInParent<CharacterStats>();
            ownerInventory = GetComponentInParent<Inventory>();
        }

        /// <summary>
        /// Try to activate the ability
        /// </summary>
        public bool TryActivate(Vector3? targetPosition = null, GameObject targetUnit = null)
        {
            // Check if on cooldown
            if (!IsReady)
            {
                Debug.Log($"{abilityName} is on cooldown: {CurrentCooldown:F1}s");
                return false;
            }

            // Check energy cost
            if (!CanAfford)
            {
                Debug.Log($"{abilityName} - not enough energy");
                return false;
            }

            // Validate targeting
            if (!ValidateTarget(targetPosition, targetUnit))
            {
                Debug.Log($"{abilityName} - invalid target");
                return false;
            }

            // Use energy
            ownerStats.UseEnergy(energyCost);

            // Set cooldown
            lastUseTime = Time.time;

            // Execute ability
            Execute(targetPosition, targetUnit);

            OnAbilityActivated.Invoke();
            return true;
        }

        /// <summary>
        /// Validate that the target is valid for this ability
        /// </summary>
        protected virtual bool ValidateTarget(Vector3? targetPosition, GameObject targetUnit)
        {
            switch (targetingType)
            {
                case TargetingType.Instant:
                    return true;

                case TargetingType.Skillshot:
                case TargetingType.PointTarget:
                    if (!targetPosition.HasValue) return false;
                    float distance = Vector3.Distance(owner.transform.position, targetPosition.Value);
                    return distance <= range * 1.1f; // Small tolerance

                case TargetingType.UnitTarget:
                    if (targetUnit == null) return false;
                    float unitDistance = Vector3.Distance(owner.transform.position, targetUnit.transform.position);
                    return unitDistance <= range * 1.1f;

                case TargetingType.Global:
                    return targetPosition.HasValue || targetUnit != null;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Execute the ability - override in subclasses
        /// </summary>
        protected abstract void Execute(Vector3? targetPosition, GameObject targetUnit);

        /// <summary>
        /// Calculate total damage for this ability
        /// </summary>
        protected float CalculateDamage()
        {
            float damage = GetBaseDamage();
            damage += ownerStats.WeaponPower * weaponRatio;
            damage += ownerStats.CrystalPower * crystalRatio;
            damage *= ownerStats.DamageMultiplier;
            return damage;
        }

        protected float GetBaseDamage()
        {
            if (damagePerLevel != null && currentLevel > 0 && currentLevel <= damagePerLevel.Length)
                return damagePerLevel[currentLevel - 1];
            return baseDamage;
        }

        /// <summary>
        /// Reduce cooldown by amount (for Twirling Silver effect)
        /// </summary>
        public void ReduceCooldown(float amount)
        {
            lastUseTime -= amount;
        }

        /// <summary>
        /// Reset cooldown completely
        /// </summary>
        public void ResetCooldown()
        {
            lastUseTime = -100f;
        }

        /// <summary>
        /// Level up the ability
        /// </summary>
        public virtual void LevelUp()
        {
            currentLevel = Mathf.Min(currentLevel + 1, 5);
            OnLevelUp();
        }

        /// <summary>
        /// Called when the ability levels up - override in subclasses to update stats
        /// </summary>
        protected virtual void OnLevelUp() { }

        /// <summary>
        /// Called when owner lands a basic attack - for abilities that interact with attacks
        /// </summary>
        public virtual void OnBasicAttackHit() { }

        /// <summary>
        /// Get targeting indicator data for UI
        /// </summary>
        public virtual TargetingIndicatorData GetTargetingIndicator()
        {
            return new TargetingIndicatorData
            {
                type = targetingType,
                range = range,
                radius = radius
            };
        }

        public struct TargetingIndicatorData
        {
            public TargetingType type;
            public float range;
            public float radius;
        }
    }
}
