using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Taka's Ultimate - Dash to target dealing massive burst damage.
    /// Extra damage if target is wounded.
    /// </summary>
    public class XRetsu : AbilityBase
    {
        [Header("X-Retsu - Vainglory Stats")]
        [SerializeField] private float dashSpeed = 25f;
        [SerializeField] private float woundedBonusDamage = 0.25f; // 25% bonus vs wounded targets

        // Level-based stats
        private float[] damages = { 350f, 500f, 650f };
        private float[] cooldowns = { 70f, 60f, 50f };
        private float[] energyCosts = { 100f, 100f, 100f };

        private bool isDashing = false;
        private Vector3 dashDirection;
        private float dashStartTime;
        private float dashDuration;
        private GameObject targetUnit;
        private Vector3 dashStartPos;

        protected override void Awake()
        {
            base.Awake();

            abilityName = "X-Retsu";
            abilityIndex = 2;
            targetingType = TargetingType.UnitTarget;
            range = 6f;
            baseDamage = damages[0];
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
            weaponRatio = 1.3f; // 130% WP ratio
            crystalRatio = 0.5f; // 50% CP ratio
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 2);
            baseDamage = damages[lvl];
            energyCost = energyCosts[lvl];
            cooldown = cooldowns[lvl];
        }

        private void Update()
        {
            if (isDashing)
            {
                UpdateDash();
            }
        }

        protected override void Execute(Vector3? targetPosition, GameObject target)
        {
            if (target == null) return;

            targetUnit = target;
            dashStartPos = owner.transform.position;
            dashDirection = (target.transform.position - owner.transform.position).normalized;
            float distance = Vector3.Distance(owner.transform.position, target.transform.position);
            dashDuration = distance / dashSpeed;
            dashStartTime = Time.time;
            isDashing = true;

            // Face the target
            owner.transform.rotation = Quaternion.LookRotation(dashDirection);

            Debug.Log($"X-Retsu: Dashing to {target.name}");
        }

        private void UpdateDash()
        {
            float elapsed = Time.time - dashStartTime;
            float progress = elapsed / dashDuration;

            if (progress >= 1f || targetUnit == null)
            {
                CompleteDash();
                return;
            }

            // Move owner towards target
            if (targetUnit != null)
            {
                Vector3 direction = (targetUnit.transform.position - owner.transform.position).normalized;
                Vector3 newPos = owner.transform.position + direction * dashSpeed * Time.deltaTime;
                owner.transform.position = newPos;

                // Check if reached target
                float distanceToTarget = Vector3.Distance(owner.transform.position, targetUnit.transform.position);
                if (distanceToTarget < 1f)
                {
                    DealDamage(targetUnit);
                    CompleteDash();
                }
            }
        }

        private void CompleteDash()
        {
            isDashing = false;
            targetUnit = null;

            // Notify TakaHero for Ki stack
            var takaHero = owner.GetComponent<TakaHero>();
            if (takaHero != null)
            {
                takaHero.OnAbilityHit();
            }

            Debug.Log("X-Retsu complete");
        }

        private void DealDamage(GameObject target)
        {
            var targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                float damage = CalculateDamage();

                // Bonus damage vs wounded targets (below 50% HP)
                if (targetStats.HealthPercent < 0.5f)
                {
                    damage *= (1f + woundedBonusDamage);
                    Debug.Log("X-Retsu wounded bonus!");
                }

                targetStats.TakeDamage(damage, damageType, owner.gameObject);
                Debug.Log($"X-Retsu dealt {damage:F0} damage to {target.name}");

                // Mortal wounds visual (optional)
                CreateImpactEffect(target.transform.position);
            }
        }

        private void CreateImpactEffect(Vector3 position)
        {
            // Create X-shaped slash effect
            for (int i = 0; i < 2; i++)
            {
                var slash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slash.transform.position = position + Vector3.up;
                slash.transform.localScale = new Vector3(0.1f, 0.1f, 2f);
                slash.transform.rotation = Quaternion.Euler(0, 45 + i * 90, 0);

                var renderer = slash.GetComponent<Renderer>();
                renderer.material.color = new Color(1f, 0.3f, 0.3f); // Red

                Destroy(slash.GetComponent<Collider>());
                Destroy(slash, 0.3f);
            }
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
