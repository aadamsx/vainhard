using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Krul's Ultimate - Throws sword in a line, stunning and pulling first hero hit.
    /// </summary>
    public class FromHellsHeart : AbilityBase
    {
        [Header("From Hell's Heart - Vainglory Stats")]
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float stunDuration = 1.5f;
        [SerializeField] private float pullSpeed = 20f;

        // Level-based stats
        private float[] damages = { 250f, 400f, 550f };
        private float[] stunDurations = { 1.5f, 1.75f, 2f };
        private float[] cooldowns = { 80f, 70f, 60f };
        private float[] energyCosts = { 100f, 100f, 100f };

        private bool isPulling = false;
        private GameObject pullTarget;
        private float pullStartTime;
        private float pullDuration;

        protected override void Awake()
        {
            base.Awake();

            abilityName = "From Hell's Heart";
            abilityIndex = 2;
            targetingType = TargetingType.Skillshot;
            range = 9f;
            baseDamage = damages[0];
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
            weaponRatio = 1f;
            crystalRatio = 0f;
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 2);
            baseDamage = damages[lvl];
            energyCost = energyCosts[lvl];
            cooldown = cooldowns[lvl];
            stunDuration = stunDurations[lvl];
        }

        private void Update()
        {
            if (isPulling)
            {
                UpdatePull();
            }
        }

        protected override void Execute(Vector3? targetPosition, GameObject targetUnit)
        {
            if (!targetPosition.HasValue) return;

            Vector3 direction = (targetPosition.Value - owner.transform.position).normalized;
            direction.y = 0;

            // Face the direction
            owner.transform.rotation = Quaternion.LookRotation(direction);

            // Spawn sword projectile
            SpawnSword(direction);

            Debug.Log($"From Hell's Heart: Thrown towards {targetPosition.Value}");
        }

        private void SpawnSword(Vector3 direction)
        {
            Vector3 spawnPos = owner.transform.position + Vector3.up + direction * 0.5f;

            // Create sword visual
            var swordObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            swordObj.transform.position = spawnPos;
            swordObj.transform.rotation = Quaternion.LookRotation(direction);
            swordObj.transform.localScale = new Vector3(0.2f, 0.2f, 1.5f);

            var renderer = swordObj.GetComponent<Renderer>();
            renderer.material.color = new Color(0.3f, 0.7f, 0.4f); // Ghostly green

            // Setup as trigger
            var collider = swordObj.GetComponent<Collider>();
            collider.isTrigger = true;

            var rb = swordObj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Add projectile component
            var projectile = swordObj.AddComponent<Projectile>();
            float damage = CalculateDamage();
            projectile.Initialize(direction, projectileSpeed, damage, damageType, owner.gameObject, OnSwordHit, true);

            // Auto-destroy after range
            Destroy(swordObj, range / projectileSpeed + 0.5f);
        }

        private void OnSwordHit(GameObject hitObject)
        {
            // Only pull heroes
            var heroController = hitObject.GetComponent<HeroController>();
            if (heroController == null) return;

            Debug.Log($"From Hell's Heart hit {hitObject.name}!");

            // Stun the target
            var targetStats = hitObject.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                // Apply stun (move speed = 0)
                targetStats.ApplyMoveSpeedModifier(0f, stunDuration);
            }

            // Start pull
            pullTarget = hitObject;
            isPulling = true;
            pullDuration = stunDuration * 0.7f; // Pull for part of stun duration
            pullStartTime = Time.time;
        }

        private void UpdatePull()
        {
            if (pullTarget == null)
            {
                isPulling = false;
                return;
            }

            float elapsed = Time.time - pullStartTime;
            if (elapsed >= pullDuration)
            {
                isPulling = false;
                pullTarget = null;
                return;
            }

            // Pull target towards Krul
            Vector3 direction = (owner.transform.position - pullTarget.transform.position).normalized;
            float distance = Vector3.Distance(owner.transform.position, pullTarget.transform.position);

            if (distance > 1.5f)
            {
                pullTarget.transform.position += direction * pullSpeed * Time.deltaTime;
            }
        }

        public override TargetingIndicatorData GetTargetingIndicator()
        {
            return new TargetingIndicatorData
            {
                type = TargetingType.Skillshot,
                range = range,
                radius = 0.5f
            };
        }
    }
}
