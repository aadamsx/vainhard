using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Krul's A ability - Gap closer that deals damage and slows.
    /// Grants barrier on activation.
    /// </summary>
    public class DeadMansRush : AbilityBase
    {
        [Header("Dead Man's Rush - Vainglory Stats")]
        [SerializeField] private float dashSpeed = 12f;
        [SerializeField] private float slowPercent = 0.4f;
        [SerializeField] private float slowDuration = 2f;

        // Level-based stats
        private float[] damages = { 60f, 100f, 140f, 180f, 220f };
        private float[] barriers = { 100f, 140f, 180f, 220f, 300f };
        private float[] cooldowns = { 10f, 9.5f, 9f, 8.5f, 8f };
        private float[] energyCosts = { 45f, 50f, 55f, 60f, 65f };

        private bool isDashing = false;
        private Vector3 dashTarget;
        private float dashStartTime;
        private float dashDuration;
        private GameObject targetUnit;

        protected override void Awake()
        {
            base.Awake();

            abilityName = "Dead Man's Rush";
            abilityIndex = 0;
            targetingType = TargetingType.UnitTarget;
            range = 5f;
            baseDamage = damages[0];
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
            weaponRatio = 1.1f; // 110% WP ratio
            crystalRatio = 0f;
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 4);
            baseDamage = damages[lvl];
            energyCost = energyCosts[lvl];
            cooldown = cooldowns[lvl];
        }

        private float GetBarrierAmount()
        {
            return barriers[Mathf.Clamp(currentLevel - 1, 0, 4)];
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
            dashTarget = target.transform.position;
            float distance = Vector3.Distance(owner.transform.position, dashTarget);
            dashDuration = distance / dashSpeed;
            dashStartTime = Time.time;
            isDashing = true;

            // Grant barrier
            // Note: Would need barrier system in CharacterStats for full implementation
            Debug.Log($"Dead Man's Rush: Barrier granted ({GetBarrierAmount()})");

            // Face the target
            Vector3 direction = (dashTarget - owner.transform.position).normalized;
            owner.transform.rotation = Quaternion.LookRotation(direction);

            Debug.Log($"Dead Man's Rush: Dashing to {target.name}");
        }

        private void UpdateDash()
        {
            float elapsed = Time.time - dashStartTime;

            if (targetUnit == null)
            {
                CompleteDash();
                return;
            }

            // Move towards target
            Vector3 direction = (targetUnit.transform.position - owner.transform.position).normalized;
            Vector3 newPos = owner.transform.position + direction * dashSpeed * Time.deltaTime;
            owner.transform.position = newPos;

            // Check if reached target
            float distanceToTarget = Vector3.Distance(owner.transform.position, targetUnit.transform.position);
            if (distanceToTarget < 1.5f)
            {
                DealDamage(targetUnit);
                CompleteDash();
            }
            else if (elapsed > dashDuration + 0.5f)
            {
                // Timeout
                CompleteDash();
            }
        }

        private void CompleteDash()
        {
            isDashing = false;
            targetUnit = null;
            Debug.Log("Dead Man's Rush complete");
        }

        private void DealDamage(GameObject target)
        {
            var targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                float damage = CalculateDamage();
                targetStats.TakeDamage(damage, damageType, owner.gameObject);

                // Apply slow
                targetStats.ApplyMoveSpeedModifier(1f - slowPercent, slowDuration);

                Debug.Log($"Dead Man's Rush dealt {damage:F0} damage, applied {slowPercent * 100}% slow");
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
