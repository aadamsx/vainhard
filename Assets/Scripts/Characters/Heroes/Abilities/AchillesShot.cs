using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Ringo's A ability - Skillshot that damages and slows.
    /// </summary>
    public class AchillesShot : AbilityBase
    {
        [Header("Achilles Shot - Vainglory Stats")]
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileWidth = 0.5f;
        [SerializeField] private GameObject projectilePrefab;

        // Level-based stats from Vainglory
        private float[] damages = { 80f, 125f, 170f, 215f, 350f };
        private float[] slowPercents = { 0.30f, 0.35f, 0.40f, 0.45f, 0.50f };
        private float[] slowDurations = { 1.5f, 1.5f, 1.5f, 1.5f, 2.5f };
        private float[] cooldowns = { 9f, 8.5f, 8f, 7.5f, 7f };
        private float[] energyCosts = { 40f, 50f, 60f, 70f, 100f };

        protected override void Awake()
        {
            base.Awake();

            abilityName = "Achilles Shot";
            abilityIndex = 0;
            targetingType = TargetingType.Skillshot;
            range = 10f;
            baseDamage = damages[0];
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
            crystalRatio = 1.25f; // 125% CP ratio
            weaponRatio = 0f;
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 4);
            baseDamage = damages[lvl];
            energyCost = energyCosts[lvl];
            cooldown = cooldowns[lvl];
        }

        private float GetCurrentSlowPercent()
        {
            return slowPercents[Mathf.Clamp(currentLevel - 1, 0, 4)];
        }

        private float GetCurrentSlowDuration()
        {
            return slowDurations[Mathf.Clamp(currentLevel - 1, 0, 4)];
        }

        protected override void Execute(Vector3? targetPosition, GameObject targetUnit)
        {
            if (!targetPosition.HasValue) return;

            Vector3 direction = (targetPosition.Value - owner.transform.position).normalized;
            direction.y = 0;

            // Face the direction
            owner.transform.rotation = Quaternion.LookRotation(direction);

            // Spawn projectile
            SpawnProjectile(direction);

            Debug.Log($"Achilles Shot fired towards {targetPosition.Value}");
        }

        private void SpawnProjectile(Vector3 direction)
        {
            Vector3 spawnPos = owner.transform.position + Vector3.up + direction * 0.5f;

            GameObject projObj;
            if (projectilePrefab != null)
            {
                projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));
            }
            else
            {
                // Create simple placeholder projectile
                projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.position = spawnPos;
                projObj.transform.localScale = Vector3.one * 0.3f;
                projObj.GetComponent<Renderer>().material.color = Color.yellow;

                // Add collider as trigger
                var collider = projObj.GetComponent<Collider>();
                collider.isTrigger = true;

                // Add rigidbody for trigger detection
                var rb = projObj.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // Add projectile component
            var projectile = projObj.AddComponent<Projectile>();
            float damage = CalculateDamage();

            projectile.Initialize(direction, projectileSpeed, damage, damageType, owner.gameObject, OnProjectileHit);

            // Auto-destroy after range
            Destroy(projObj, range / projectileSpeed + 0.5f);
        }

        private void OnProjectileHit(GameObject hitObject)
        {
            float slowPercent = GetCurrentSlowPercent();
            float slowDuration = GetCurrentSlowDuration();

            // Apply slow
            var motor = hitObject.GetComponent<CharacterMotor>();
            if (motor != null)
            {
                motor.ApplySlow(slowPercent, slowDuration);
            }

            var stats = hitObject.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.ApplyMoveSpeedModifier(1f - slowPercent, slowDuration);
            }

            Debug.Log($"Achilles Shot hit {hitObject.name}, applied {slowPercent * 100}% slow for {slowDuration}s");
        }

        public override TargetingIndicatorData GetTargetingIndicator()
        {
            return new TargetingIndicatorData
            {
                type = TargetingType.Skillshot,
                range = range,
                radius = projectileWidth
            };
        }
    }
}
