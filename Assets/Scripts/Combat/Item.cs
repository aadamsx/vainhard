using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Defines an item that can be purchased and equipped.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "MOBA/Item")]
    public class Item : ScriptableObject
    {
        [Header("Info")]
        public string itemName;
        public string description;
        public Sprite icon;
        public int tier = 1;

        [Header("Cost")]
        public int cost;
        public Item[] buildsFrom;
        public Item[] buildsInto;

        [Header("Stats")]
        public float weaponPower;
        public float crystalPower;
        public float maxHealth;
        public float maxEnergy;
        public float attackSpeed; // Multiplier (0.1 = 10% increase)
        public float moveSpeed; // Multiplier
        public float armor;
        public float shield;
        public float cooldownReduction; // Percentage (0.1 = 10%)
        public float lifesteal; // Percentage (0.1 = 10%)
        public float critChance; // Percentage (0.2 = 20%)
        public float critDamage; // Multiplier bonus (0.5 = +50% crit damage, for 150% total)
        public float armorPierce; // Flat armor ignored
        public float armorPiercePercent; // Percentage of armor ignored (0.1 = 10%)
        public float shieldPierce; // Flat shield ignored
        public float shieldPiercePercent; // Percentage of shield ignored

        [Header("Passive")]
        public bool hasPassive;
        public string passiveName;
        [TextArea]
        public string passiveDescription;

        /// <summary>
        /// Apply this item's stats to a character
        /// </summary>
        public void ApplyStats(CharacterStats stats)
        {
            // Stats are applied through the inventory system
            // Each stat modifier uses a unique ID based on item instance
        }

        /// <summary>
        /// Get the total cost including components
        /// </summary>
        public int GetTotalCost()
        {
            int total = cost;
            if (buildsFrom != null)
            {
                foreach (var component in buildsFrom)
                {
                    if (component != null)
                    {
                        total += component.GetTotalCost();
                    }
                }
            }
            return total;
        }
    }
}
