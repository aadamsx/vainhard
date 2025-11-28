using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Taka's A ability - Dash through a target, dealing damage.
    /// Grants brief invulnerability during dash.
    /// </summary>
    public class Kaiten : AbilityBase
    {
        [Header("Kaiten - Vainglory Stats")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDistance = 5f;

        // Level-based stats
        private float[] damages = { 80f, 110f, 140f, 170f, 230f };
        private float[] cooldowns = { 12f, 11f, 10f, 9f, 8f };
        private float[] energyCosts = { 50f, 55f, 60f, 65f, 70f };

        private bool isDashing = false;
        private Vector3 dashTarget;
        private Vector3 dashDirection;
        private float dashStartTime;
        private float dashDuration;
        private GameObject targetUnit;

        protected override void Awake()
        {
            base.Awake();

            abilityName = "Kaiten";
            abilityIndex = 0;
            targetingType = TargetingType.UnitTarget;
            range = 4f;
            baseDamage = damages[0];
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
            weaponRatio = 1.4f; // 140% WP ratio
            crystalRatio = 0.8f; // 80% CP ratio
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 4);
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
            dashDirection = (target.transform.position - owner.transform.position).normalized;
            dashTarget = target.transform.position + dashDirection * 2f; // Dash through target
            dashStartTime = Time.time;
            dashDuration = dashDistance / dashSpeed;
            isDashing = true;

            // Face the target
            owner.transform.rotation = Quaternion.LookRotation(dashDirection);

            Debug.Log($"Kaiten: Dashing through {target.name}");
        }

        private void UpdateDash()
        {
            float elapsed = Time.time - dashStartTime;
            float progress = elapsed / dashDuration;

            if (progress >= 1f)
            {
                // Dash complete
                CompleteDash();
                return;
            }

            // Move owner
            Vector3 newPos = owner.transform.position + dashDirection * dashSpeed * Time.deltaTime;
            owner.transform.position = newPos;

            // Check if we passed through target
            if (targetUnit != null)
            {
                float distanceToTarget = Vector3.Distance(owner.transform.position, targetUnit.transform.position);
                if (distanceToTarget < 1.5f)
                {
                    DealDamage(targetUnit);
                    targetUnit = null; // Only damage once
                }
            }
        }

        private void CompleteDash()
        {
            isDashing = false;

            // Notify TakaHero for Ki stack
            var takaHero = owner.GetComponent<TakaHero>();
            if (takaHero != null)
            {
                takaHero.OnAbilityHit();
            }

            Debug.Log("Kaiten dash complete");
        }

        private void DealDamage(GameObject target)
        {
            var targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                float damage = CalculateDamage();
                targetStats.TakeDamage(damage, damageType, owner.gameObject);
                Debug.Log($"Kaiten dealt {damage:F0} damage to {target.name}");
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
