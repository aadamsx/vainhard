using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Manages a character's inventory of items and applies stat bonuses.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxSlots = 6;

        // Events
        public UnityEvent<Item> OnItemPurchased = new UnityEvent<Item>();
        public UnityEvent<Item> OnItemSold = new UnityEvent<Item>();
        public UnityEvent OnInventoryChanged = new UnityEvent();

        // State
        private List<Item> items = new List<Item>();
        private CharacterStats stats;

        // Calculated bonuses
        private float bonusWeaponPower;
        private float bonusCrystalPower;
        private float bonusMaxHealth;
        private float bonusMaxEnergy;
        private float bonusAttackSpeed;
        private float bonusMoveSpeed;
        private float bonusArmor;
        private float bonusShield;
        private float bonusCooldownReduction;
        private float bonusLifesteal;
        private float bonusCritChance;
        private float bonusCritDamage;
        private float bonusArmorPierce;
        private float bonusArmorPiercePercent;
        private float bonusShieldPierce;
        private float bonusShieldPiercePercent;

        public List<Item> Items => items;
        public int MaxSlots => maxSlots;
        public int UsedSlots => items.Count;
        public bool HasSpace => items.Count < maxSlots;

        // Stat accessors
        public float BonusWeaponPower => bonusWeaponPower;
        public float BonusCrystalPower => bonusCrystalPower;
        public float BonusMaxHealth => bonusMaxHealth;
        public float BonusMaxEnergy => bonusMaxEnergy;
        public float BonusAttackSpeed => bonusAttackSpeed;
        public float BonusMoveSpeed => bonusMoveSpeed;
        public float BonusArmor => bonusArmor;
        public float BonusShield => bonusShield;
        public float BonusCooldownReduction => bonusCooldownReduction;
        public float BonusLifesteal => bonusLifesteal;
        public float BonusCritChance => bonusCritChance;
        public float BonusCritDamage => bonusCritDamage;
        public float BonusArmorPierce => bonusArmorPierce;
        public float BonusArmorPiercePercent => bonusArmorPiercePercent;
        public float BonusShieldPierce => bonusShieldPierce;
        public float BonusShieldPiercePercent => bonusShieldPiercePercent;

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();
        }

        /// <summary>
        /// Try to purchase an item
        /// </summary>
        public bool TryPurchase(Item item)
        {
            if (item == null) return false;
            if (!HasSpace) return false;
            if (stats == null) return false;

            // Check if we have gold
            if (!stats.SpendGold(item.cost))
            {
                return false;
            }

            // Add item
            items.Add(item);
            RecalculateBonuses();

            OnItemPurchased.Invoke(item);
            OnInventoryChanged.Invoke();

            return true;
        }

        /// <summary>
        /// Add an item directly without gold check (used by shop after gold already spent)
        /// </summary>
        public void AddItemDirectly(Item item)
        {
            if (item == null) return;
            if (!HasSpace) return;

            items.Add(item);
            RecalculateBonuses();

            OnItemPurchased.Invoke(item);
            OnInventoryChanged.Invoke();

            Debug.Log($"Inventory: Added {item.itemName}, now has {items.Count} items. " +
                      $"Lifesteal: {bonusLifesteal * 100}%, Crit: {bonusCritChance * 100}%, ArmorPierce: {bonusArmorPierce}");
        }

        /// <summary>
        /// Sell an item for partial gold refund
        /// </summary>
        public bool SellItem(int slotIndex, float refundPercent = 0.7f)
        {
            if (slotIndex < 0 || slotIndex >= items.Count) return false;

            Item item = items[slotIndex];
            int refund = Mathf.RoundToInt(item.cost * refundPercent);

            items.RemoveAt(slotIndex);
            stats.AddGold(refund);
            RecalculateBonuses();

            OnItemSold.Invoke(item);
            OnInventoryChanged.Invoke();

            return true;
        }

        /// <summary>
        /// Check if player can afford an item
        /// </summary>
        public bool CanAfford(Item item)
        {
            return stats != null && stats.Gold >= item.cost;
        }

        private void RecalculateBonuses()
        {
            // Reset bonuses
            bonusWeaponPower = 0;
            bonusCrystalPower = 0;
            bonusMaxHealth = 0;
            bonusMaxEnergy = 0;
            bonusAttackSpeed = 0;
            bonusMoveSpeed = 0;
            bonusArmor = 0;
            bonusShield = 0;
            bonusCooldownReduction = 0;
            bonusLifesteal = 0;
            bonusCritChance = 0;
            bonusCritDamage = 0;
            bonusArmorPierce = 0;
            bonusArmorPiercePercent = 0;
            bonusShieldPierce = 0;
            bonusShieldPiercePercent = 0;

            // Sum up all item bonuses
            foreach (var item in items)
            {
                if (item == null) continue;

                bonusWeaponPower += item.weaponPower;
                bonusCrystalPower += item.crystalPower;
                bonusMaxHealth += item.maxHealth;
                bonusMaxEnergy += item.maxEnergy;
                bonusAttackSpeed += item.attackSpeed;
                bonusMoveSpeed += item.moveSpeed;
                bonusArmor += item.armor;
                bonusShield += item.shield;
                bonusCooldownReduction += item.cooldownReduction;
                bonusLifesteal += item.lifesteal;
                bonusCritChance += item.critChance;
                bonusCritDamage += item.critDamage;
                bonusArmorPierce += item.armorPierce;
                bonusArmorPiercePercent += item.armorPiercePercent;
                bonusShieldPierce += item.shieldPierce;
                bonusShieldPiercePercent += item.shieldPiercePercent;
            }

            // Cap cooldown reduction at 40%
            bonusCooldownReduction = Mathf.Min(bonusCooldownReduction, 0.4f);
            // Cap crit chance at 100%
            bonusCritChance = Mathf.Min(bonusCritChance, 1f);
        }

        /// <summary>
        /// Get item at slot
        /// </summary>
        public Item GetItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= items.Count) return null;
            return items[slotIndex];
        }
    }
}
