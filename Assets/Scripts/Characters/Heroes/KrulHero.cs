using UnityEngine;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Characters
{
    /// <summary>
    /// Krul - Undead warrior with lifesteal and stacking marks.
    /// Passive: Shadows Empower Me - Basic attacks heal Krul and apply Weakness stacks.
    /// </summary>
    public class KrulHero : MonoBehaviour
    {
        [Header("Shadows Empower Me Passive")]
        [SerializeField] private float lifestealPercent = 0.10f; // 10% lifesteal
        [SerializeField] private int maxWeaknessStacks = 8;
        [SerializeField] private float weaknessStackDuration = 4f;
        [SerializeField] private float damagePerStack = 0.025f; // 2.5% bonus damage per stack

        [Header("Visual")]
        [SerializeField] private Color empoweredColor = new Color(0.4f, 0.8f, 0.4f);

        private CharacterStats ownerStats;
        private HeroController heroController;

        // Weakness tracking per target
        private System.Collections.Generic.Dictionary<GameObject, WeaknessData> weaknessTargets
            = new System.Collections.Generic.Dictionary<GameObject, WeaknessData>();

        private class WeaknessData
        {
            public int stacks;
            public float expireTime;
        }

        public float LifestealPercent => lifestealPercent;

        private void Awake()
        {
            ownerStats = GetComponent<CharacterStats>();
            heroController = GetComponent<HeroController>();
        }

        private void Update()
        {
            CleanupExpiredWeakness();
        }

        private void CleanupExpiredWeakness()
        {
            var expired = new System.Collections.Generic.List<GameObject>();
            foreach (var kvp in weaknessTargets)
            {
                if (kvp.Key == null || Time.time > kvp.Value.expireTime)
                {
                    expired.Add(kvp.Key);
                }
            }
            foreach (var key in expired)
            {
                weaknessTargets.Remove(key);
            }
        }

        /// <summary>
        /// Called when Krul's basic attack hits. Applies weakness and heals.
        /// </summary>
        public void OnBasicAttackHit(GameObject target, float damageDealt)
        {
            // Apply weakness stack
            if (target != null)
            {
                ApplyWeaknessStack(target);
            }

            // Lifesteal heal
            float healAmount = damageDealt * lifestealPercent;
            ownerStats.Heal(healAmount);

            Debug.Log($"Krul healed {healAmount:F0} from attack");
        }

        private void ApplyWeaknessStack(GameObject target)
        {
            if (!weaknessTargets.ContainsKey(target))
            {
                weaknessTargets[target] = new WeaknessData { stacks = 0 };
            }

            var data = weaknessTargets[target];
            if (data.stacks < maxWeaknessStacks)
            {
                data.stacks++;
            }
            data.expireTime = Time.time + weaknessStackDuration;

            Debug.Log($"Weakness on {target.name}: {data.stacks}/{maxWeaknessStacks}");
        }

        /// <summary>
        /// Get weakness stacks on a target for bonus damage calculation
        /// </summary>
        public int GetWeaknessStacks(GameObject target)
        {
            if (target != null && weaknessTargets.ContainsKey(target))
            {
                var data = weaknessTargets[target];
                if (Time.time <= data.expireTime)
                {
                    return data.stacks;
                }
            }
            return 0;
        }

        /// <summary>
        /// Modify attack damage based on weakness stacks
        /// </summary>
        public float ModifyAttackDamage(float baseDamage, GameObject target)
        {
            int stacks = GetWeaknessStacks(target);
            float bonusMult = 1f + (stacks * damagePerStack);
            return baseDamage * bonusMult;
        }

        /// <summary>
        /// Consume all weakness stacks (for Spectral Smite)
        /// Returns stack count before consuming
        /// </summary>
        public int ConsumeWeaknessStacks(GameObject target)
        {
            int stacks = GetWeaknessStacks(target);
            if (target != null && weaknessTargets.ContainsKey(target))
            {
                weaknessTargets.Remove(target);
            }
            return stacks;
        }

        /// <summary>
        /// Reset for respawn
        /// </summary>
        public void Reset()
        {
            weaknessTargets.Clear();
        }
    }
}
