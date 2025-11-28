using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Krul's B ability - Consumes Weakness stacks for damage and healing.
    /// Damage and healing scale with consumed stacks.
    /// </summary>
    public class SpectralSmite : AbilityBase
    {
        [Header("Spectral Smite - Vainglory Stats")]
        [SerializeField] private float damagePerStack = 20f;
        [SerializeField] private float healPerStack = 15f;

        // Level-based stats
        private float[] baseDamages = { 40f, 80f, 120f, 160f, 200f };
        private float[] baseHeals = { 50f, 80f, 110f, 140f, 200f };
        private float[] cooldowns = { 8f, 7.5f, 7f, 6.5f, 6f };
        private float[] energyCosts = { 30f, 35f, 40f, 45f, 50f };

        protected override void Awake()
        {
            base.Awake();

            abilityName = "Spectral Smite";
            abilityIndex = 1;
            targetingType = TargetingType.UnitTarget;
            range = 3f; // Melee range
            baseDamage = baseDamages[0];
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
            weaponRatio = 0.7f;
            crystalRatio = 0.8f;
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 4);
            baseDamage = baseDamages[lvl];
            energyCost = energyCosts[lvl];
            cooldown = cooldowns[lvl];
        }

        private float GetBaseHeal()
        {
            return baseHeals[Mathf.Clamp(currentLevel - 1, 0, 4)];
        }

        protected override void Execute(Vector3? targetPosition, GameObject target)
        {
            if (target == null) return;

            // Get Krul hero to check weakness stacks
            var krulHero = owner.GetComponent<KrulHero>();
            int stacks = 0;
            if (krulHero != null)
            {
                stacks = krulHero.ConsumeWeaknessStacks(target);
            }

            // Calculate damage with stack bonus
            float damage = CalculateDamage() + (stacks * damagePerStack);

            // Deal damage
            var targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage, damageType, owner.gameObject);
            }

            // Heal Krul based on stacks
            float healAmount = GetBaseHeal() + (stacks * healPerStack);
            ownerStats.Heal(healAmount);

            // Visual effect
            CreateSmiteEffect(target.transform.position);

            Debug.Log($"Spectral Smite: {damage:F0} damage, healed {healAmount:F0} (consumed {stacks} stacks)");
        }

        private void CreateSmiteEffect(Vector3 position)
        {
            // Create ghostly slash effect
            var effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.transform.position = position + Vector3.up;
            effect.transform.localScale = Vector3.one * 0.8f;

            var renderer = effect.GetComponent<Renderer>();
            renderer.material.color = new Color(0.3f, 0.8f, 0.5f, 0.7f); // Ghostly green

            Destroy(effect.GetComponent<Collider>());
            Destroy(effect, 0.3f);
        }

        public override TargetingIndicatorData GetTargetingIndicator()
        {
            return new TargetingIndicatorData
            {
                type = TargetingType.UnitTarget,
                range = range,
                radius = 0
            };
        }
    }
}
