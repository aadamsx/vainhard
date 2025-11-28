using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using VaingloryMoba.UI;

namespace VaingloryMoba.Characters
{
    /// <summary>
    /// Manages all character stats including health, energy, damage, and modifiers.
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        [Header("Base Stats - Ringo reference values")]
        [SerializeField] private float maxHealth = 703f;
        [SerializeField] private float maxEnergy = 163f;
        [SerializeField] private float healthRegen = 3f; // per second
        [SerializeField] private float energyRegen = 5f; // per second
        [SerializeField] private float weaponPower = 71f;
        [SerializeField] private float crystalPower = 0f;
        [SerializeField] private float attackSpeed = 1f; // 100%
        [SerializeField] private float moveSpeed = 3.1f;
        [SerializeField] private float attackRange = 6.2f;
        [SerializeField] private float armor = 20f;
        [SerializeField] private float shield = 20f;

        [Header("Per Level Scaling")]
        [SerializeField] private float healthPerLevel = 69f;
        [SerializeField] private float energyPerLevel = 22f;
        [SerializeField] private float armorPerLevel = 6f;
        [SerializeField] private float shieldPerLevel = 6f;
        [SerializeField] private float weaponPerLevel = 6f;
        [SerializeField] private float attackSpeedPerLevel = 0.03f; // 3%

        [Header("Level")]
        [SerializeField] private int level = 1;
        [SerializeField] private float experience = 0f;
        [SerializeField] private float[] expToLevel = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700, 3250, 3850 };

        // Current values
        private float currentHealth;
        private float currentEnergy;
        private int gold;

        // Stat modifiers (buff/debuff)
        private List<StatModifier> modifiers = new List<StatModifier>();

        // Flat bonuses from items (permanent, tracked by id)
        private Dictionary<string, FlatBonus> flatBonuses = new Dictionary<string, FlatBonus>();

        public class FlatBonus
        {
            public StatType type;
            public float value;
        }

        // Events
        public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>(); // current, max
        public UnityEvent<float, float> OnEnergyChanged = new UnityEvent<float, float>();
        public UnityEvent<int> OnGoldChanged = new UnityEvent<int>();
        public UnityEvent<int> OnLevelUp = new UnityEvent<int>();
        public UnityEvent OnDeath = new UnityEvent();
        public UnityEvent<GameObject> OnDeathWithKiller = new UnityEvent<GameObject>(); // passes killer

        // Track last damage source for kill credit
        private GameObject lastDamageSource;

        // Cached multipliers
        private float moveSpeedMultiplier = 1f;
        private float attackSpeedMultiplier = 1f;
        private float damageMultiplier = 1f;

        public class StatModifier
        {
            public string id;
            public StatType type;
            public float value;
            public float duration;
            public float startTime;
            public bool isExpired => duration > 0 && Time.time > startTime + duration;
        }

        public enum StatType
        {
            MoveSpeed,
            AttackSpeed,
            WeaponPower,
            CrystalPower,
            Armor,
            Shield,
            MaxHealth,
            HealthRegen,
            EnergyRegen,
            DamageDealt,
            DamageTaken
        }

        private void Awake()
        {
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
        }

        // Passive gold income tracking
        private float passiveGoldTimer = 0f;
        private bool isHero = false;

        public void SetIsHero(bool hero) { isHero = hero; }

        private void Update()
        {
            // Regeneration
            if (currentHealth < maxHealth && currentHealth > 0)
            {
                Heal(healthRegen * Time.deltaTime, false);
            }

            if (currentEnergy < maxEnergy)
            {
                RestoreEnergy(energyRegen * Time.deltaTime, false);
            }

            // Passive gold income (1 gold/sec) - only for heroes
            if (isHero && currentHealth > 0)
            {
                passiveGoldTimer += Time.deltaTime;
                if (passiveGoldTimer >= 1f)
                {
                    AddGold(1);
                    passiveGoldTimer -= 1f;
                }
            }

            // Clean up expired modifiers
            CleanupModifiers();
        }

        private void CleanupModifiers()
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (modifiers[i].isExpired)
                {
                    modifiers.RemoveAt(i);
                }
            }
            RecalculateMultipliers();
        }

        private void RecalculateMultipliers()
        {
            moveSpeedMultiplier = 1f;
            attackSpeedMultiplier = 1f;
            damageMultiplier = 1f;

            foreach (var mod in modifiers)
            {
                if (mod.isExpired) continue;

                switch (mod.type)
                {
                    case StatType.MoveSpeed:
                        moveSpeedMultiplier *= mod.value;
                        break;
                    case StatType.AttackSpeed:
                        attackSpeedMultiplier *= mod.value;
                        break;
                    case StatType.DamageDealt:
                        damageMultiplier *= mod.value;
                        break;
                }
            }
        }

        #region Health

        public void TakeDamage(float amount, DamageType type, GameObject source = null)
        {
            TakeDamageWithPierce(amount, type, source, 0f, 0f);
        }

        /// <summary>
        /// Take damage with armor/shield penetration
        /// </summary>
        public void TakeDamageWithPierce(float amount, DamageType type, GameObject source, float flatPierce, float percentPierce)
        {
            if (currentHealth <= 0) return;

            // Track damage source for kill credit (last-hit mechanic)
            if (source != null)
            {
                lastDamageSource = source;
            }

            // Get base defense
            float baseDefense = type == DamageType.Physical ? Armor : Shield;

            // Apply penetration: first flat, then percentage
            float effectiveDefense = baseDefense;
            if (type != DamageType.True)
            {
                // Flat pierce reduces defense directly
                effectiveDefense = Mathf.Max(0, effectiveDefense - flatPierce);
                // Percent pierce ignores a percentage of remaining defense
                effectiveDefense *= (1f - percentPierce);
            }
            else
            {
                // True damage ignores all defense
                effectiveDefense = 0f;
            }

            // Calculate damage reduction with effective defense
            float damageReduction = effectiveDefense / (effectiveDefense + 100f); // Diminishing returns formula
            float finalDamage = amount * (1f - damageReduction);

            // Apply damage taken modifiers
            float damageTakenMod = GetModifierValue(StatType.DamageTaken);
            finalDamage *= damageTakenMod;

            currentHealth = Mathf.Max(0, currentHealth - finalDamage);
            OnHealthChanged.Invoke(currentHealth, MaxHealth);

            // Show floating damage number
            DamageNumber.Spawn(transform.position, finalDamage);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public GameObject LastDamageSource => lastDamageSource;

        public void Heal(float amount, bool notify = true)
        {
            if (currentHealth <= 0) return;

            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);

            if (notify)
            {
                OnHealthChanged.Invoke(currentHealth, MaxHealth);
            }
        }

        public void FullHeal()
        {
            currentHealth = MaxHealth;
            currentEnergy = MaxEnergy;
            OnHealthChanged.Invoke(currentHealth, MaxHealth);
            OnEnergyChanged.Invoke(currentEnergy, MaxEnergy);
        }

        private void Die()
        {
            OnDeath.Invoke();
            OnDeathWithKiller.Invoke(lastDamageSource);
        }

        #endregion

        #region Energy

        public bool UseEnergy(float amount)
        {
            if (currentEnergy < amount)
                return false;

            currentEnergy -= amount;
            OnEnergyChanged.Invoke(currentEnergy, MaxEnergy);
            return true;
        }

        public void RestoreEnergy(float amount, bool notify = true)
        {
            currentEnergy = Mathf.Min(MaxEnergy, currentEnergy + amount);

            if (notify)
            {
                OnEnergyChanged.Invoke(currentEnergy, MaxEnergy);
            }
        }

        public bool HasEnergy(float amount)
        {
            return currentEnergy >= amount;
        }

        #endregion

        #region Experience & Gold

        public void AddExperience(float amount)
        {
            experience += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            if (level >= expToLevel.Length) return;

            while (level < expToLevel.Length && experience >= expToLevel[level])
            {
                level++;
                OnLevelUp.Invoke(level);

                // Increase stats on level up
                maxHealth += 50f;
                maxEnergy += 20f;
                weaponPower += 5f;
                armor += 3f;
                shield += 3f;

                // Heal a bit on level up
                Heal(100f);
            }
        }

        public void AddGold(int amount)
        {
            gold += amount;
            OnGoldChanged.Invoke(gold);
        }

        public bool SpendGold(int amount)
        {
            if (gold < amount) return false;
            gold -= amount;
            OnGoldChanged.Invoke(gold);
            return true;
        }

        #endregion

        #region Modifiers

        public void ApplyModifier(string id, StatType type, float value, float duration = -1f)
        {
            // Remove existing modifier with same ID
            modifiers.RemoveAll(m => m.id == id);

            modifiers.Add(new StatModifier
            {
                id = id,
                type = type,
                value = value,
                duration = duration,
                startTime = Time.time
            });

            RecalculateMultipliers();
        }

        public void RemoveModifier(string id)
        {
            modifiers.RemoveAll(m => m.id == id);
            RecalculateMultipliers();
        }

        public void ApplyMoveSpeedModifier(float multiplier, float duration)
        {
            ApplyModifier($"slow_{Time.time}", StatType.MoveSpeed, multiplier, duration);
        }

        public void ApplyAttackSpeedModifier(float multiplier, float duration)
        {
            ApplyModifier($"attackspeed_{Time.time}", StatType.AttackSpeed, multiplier, duration);
        }

        private float GetModifierValue(StatType type)
        {
            float value = 1f;
            foreach (var mod in modifiers)
            {
                if (!mod.isExpired && mod.type == type)
                {
                    value *= mod.value;
                }
            }
            return value;
        }

        #endregion

        #region Properties

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth + GetFlatBonus(StatType.MaxHealth);
        public float HealthPercent => currentHealth / MaxHealth;
        public bool IsAlive => currentHealth > 0;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float EnergyPercent => currentEnergy / MaxEnergy;

        public float WeaponPower => weaponPower + GetFlatBonus(StatType.WeaponPower);
        public float CrystalPower => crystalPower + GetFlatBonus(StatType.CrystalPower);
        public float AttackSpeed => attackSpeed * attackSpeedMultiplier;
        public float MoveSpeed => moveSpeed;
        public float MoveSpeedMultiplier => moveSpeedMultiplier;
        public float AttackRange => attackRange;
        public float Armor => armor + GetFlatBonus(StatType.Armor);
        public float Shield => shield + GetFlatBonus(StatType.Shield);
        public float DamageMultiplier => damageMultiplier;

        public int Level => level;
        public float Experience => experience;
        public int Gold => gold;

        // Set base stats for minions/monsters
        public void SetBaseHealth(float health)
        {
            maxHealth = health;
            currentHealth = health;
        }

        public void SetBaseStats(float health, float damage, float armor = 0f)
        {
            maxHealth = health;
            currentHealth = health;
            weaponPower = damage;
            this.armor = armor;
            this.shield = 0f;
        }

        public void AddFlatBonus(StatType type, float value, string id)
        {
            flatBonuses[id] = new FlatBonus { type = type, value = value };
        }

        public void RemoveFlatBonus(string id)
        {
            flatBonuses.Remove(id);
        }

        private float GetFlatBonus(StatType type)
        {
            float total = 0f;
            foreach (var bonus in flatBonuses.Values)
            {
                if (bonus.type == type)
                    total += bonus.value;
            }
            return total;
        }

        #endregion

        public enum DamageType
        {
            Physical,
            Crystal,
            True
        }
    }
}
