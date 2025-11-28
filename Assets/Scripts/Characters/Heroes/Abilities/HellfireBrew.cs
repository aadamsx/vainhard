using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Ringo's Ultimate - Point-and-click homing fireball that can be body-blocked.
    /// Deals initial damage plus burn damage over time.
    /// </summary>
    public class HellfireBrew : AbilityBase
    {
        [Header("Hellfire Brew - Vainglory Stats")]
        [SerializeField] private float burnDuration = 3f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private float castTime = 0.5f; // Drinking animation
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject castEffectPrefab;

        // Level-based stats from Vainglory
        private float[] damages = { 250f, 365f, 480f };
        private float[] burnDamages = { 30f, 50f, 70f }; // Per second for 3 seconds
        private float[] cooldowns = { 100f, 85f, 70f };
        private float[] energyCosts = { 150f, 175f, 200f };

        private bool isCasting;
        private float castEndTime;
        private GameObject pendingTarget;

        protected override void Awake()
        {
            base.Awake();

            abilityName = "Hellfire Brew";
            abilityIndex = 3; // Ultimate
            targetingType = TargetingType.UnitTarget;
            range = 1000f; // Global range - Vainglory value
            baseDamage = damages[0];
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
            crystalRatio = 1.30f; // 130% CP ratio from Vainglory
            weaponRatio = 0f;
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 2); // Ult has 3 levels
            baseDamage = damages[lvl];
            energyCost = energyCosts[lvl];
            cooldown = cooldowns[lvl];
        }

        private float GetCurrentBurnDamage()
        {
            return burnDamages[Mathf.Clamp(currentLevel - 1, 0, 2)];
        }

        private void Update()
        {
            // Handle cast time
            if (isCasting && Time.time >= castEndTime)
            {
                FinishCast();
            }
        }

        protected override void Execute(Vector3? targetPosition, GameObject targetUnit)
        {
            if (targetUnit == null)
            {
                Debug.Log("Hellfire Brew requires a target!");
                return;
            }

            // Start cast (drinking animation)
            StartCast(targetUnit);
        }

        private void StartCast(GameObject target)
        {
            isCasting = true;
            castEndTime = Time.time + castTime;
            pendingTarget = target;

            // Stop movement during cast
            owner.Motor.StopMoving();

            // Face target
            Vector3 direction = (target.transform.position - owner.transform.position).normalized;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                owner.transform.rotation = Quaternion.LookRotation(direction);
            }

            // Spawn cast effect
            if (castEffectPrefab != null)
            {
                var effect = Instantiate(castEffectPrefab, owner.transform.position + Vector3.up, Quaternion.identity);
                Destroy(effect, castTime + 0.5f);
            }

            Debug.Log($"Hellfire Brew casting on {target.name}...");
        }

        private void FinishCast()
        {
            isCasting = false;

            if (pendingTarget == null)
            {
                Debug.Log("Hellfire Brew target lost!");
                return;
            }

            // Spawn the fireball
            SpawnFireball(pendingTarget);
            pendingTarget = null;
        }

        private void SpawnFireball(GameObject target)
        {
            Vector3 spawnPos = owner.transform.position + Vector3.up * 1.5f;

            GameObject projObj;
            if (projectilePrefab != null)
            {
                projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Create placeholder fireball
                projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.position = spawnPos;
                projObj.transform.localScale = Vector3.one * 0.5f;
                projObj.GetComponent<Renderer>().material.color = new Color(1f, 0.3f, 0f);

                // Add glow effect (additional sphere)
                var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                glow.transform.SetParent(projObj.transform);
                glow.transform.localPosition = Vector3.zero;
                glow.transform.localScale = Vector3.one * 1.5f;
                var glowMat = glow.GetComponent<Renderer>().material;
                glowMat.color = new Color(1f, 0.5f, 0f, 0.3f);
                Destroy(glow.GetComponent<Collider>());

                // Add collider as trigger
                var collider = projObj.GetComponent<Collider>();
                collider.isTrigger = true;

                // Add rigidbody
                var rb = projObj.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // Add and configure projectile component
            var projectile = projObj.AddComponent<Projectile>();
            float damage = CalculateDamage();

            projectile.InitializeHoming(
                target.transform,
                projectileSpeed,
                damage,
                damageType,
                owner.gameObject,
                true, // Can be body-blocked
                OnFireballHit
            );

            Debug.Log($"Hellfire Brew launched at {target.name}!");
        }

        private void OnFireballHit(GameObject hitObject)
        {
            // Apply burn damage over time
            var stats = hitObject.GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Start burn coroutine
                StartCoroutine(ApplyBurn(stats));
            }

            Debug.Log($"Hellfire Brew hit {hitObject.name}! Applying burn.");
        }

        private System.Collections.IEnumerator ApplyBurn(CharacterStats target)
        {
            float tickInterval = 0.5f;
            float burnPerSecond = GetCurrentBurnDamage();
            float damagePerTick = burnPerSecond * tickInterval;
            float elapsed = 0f;

            while (elapsed < burnDuration && target != null && target.IsAlive)
            {
                target.TakeDamage(damagePerTick, CharacterStats.DamageType.Crystal, owner.gameObject);
                elapsed += tickInterval;
                yield return new WaitForSeconds(tickInterval);
            }
        }

        protected override bool ValidateTarget(Vector3? targetPosition, GameObject targetUnit)
        {
            // Must target an enemy hero
            if (targetUnit == null) return false;

            var targetHero = targetUnit.GetComponent<HeroController>();
            if (targetHero == null) return false;

            // Must be enemy
            if (targetHero.Team == owner.Team) return false;

            return true;
        }

        public override TargetingIndicatorData GetTargetingIndicator()
        {
            return new TargetingIndicatorData
            {
                type = TargetingType.UnitTarget,
                range = range,
                radius = 0f
            };
        }
    }
}
