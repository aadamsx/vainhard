using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Ringo's B ability - Self-buff that increases attack speed
    /// and reduces other ability cooldowns on basic attack hits.
    /// </summary>
    public class TwirlingSilver : AbilityBase
    {
        [Header("Twirling Silver - Vainglory Stats")]
        [SerializeField] private float duration = 6f;
        [SerializeField] private float cooldownReductionPerHit = 0.6f; // Vainglory value
        [SerializeField] private GameObject buffEffectPrefab;

        // Level-based stats from Vainglory
        private float[] attackSpeedBonuses = { 0.50f, 0.60f, 0.70f, 0.80f, 1.00f }; // 50/60/70/80/100%
        private float[] energyCosts = { 40f, 45f, 50f, 55f, 60f };

        private bool isActive;
        private float buffEndTime;
        private GameObject activeBuffEffect;

        public bool IsActive => isActive;

        protected override void Awake()
        {
            base.Awake();

            abilityName = "Twirling Silver";
            abilityIndex = 1;
            targetingType = TargetingType.Instant;
            baseDamage = 5f; // Crystal damage per attack
            energyCost = energyCosts[0];
            cooldown = 12f; // Base cooldown
            cooldownPerLevel = new float[] { 12f, 11f, 10f, 9f, 8f }; // Vainglory values
            crystalRatio = 0.8f;
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 4);
            energyCost = energyCosts[lvl];
        }

        private float GetCurrentAttackSpeedBonus()
        {
            return attackSpeedBonuses[Mathf.Clamp(currentLevel - 1, 0, 4)];
        }

        private void Update()
        {
            // Check if buff expired
            if (isActive && Time.time >= buffEndTime)
            {
                DeactivateBuff();
            }
        }

        protected override void Execute(Vector3? targetPosition, GameObject targetUnit)
        {
            Debug.Log($"[TWIRLING SILVER] Execute called - Owner: {owner?.gameObject.name ?? "null"}, Owner Team: {owner?.Team}, IsPlayerControlled: {owner?.IsPlayerControlled}");
            ActivateBuff();
            Debug.Log($"[TWIRLING SILVER] Buff activated on {owner?.gameObject.name}! +{GetCurrentAttackSpeedBonus() * 100}% attack speed for {duration}s");
        }

        private void ActivateBuff()
        {
            isActive = true;
            buffEndTime = Time.time + duration;

            // Apply attack speed modifier
            float atkSpeedBonus = GetCurrentAttackSpeedBonus();
            float multiplier = 1f + atkSpeedBonus;
            ownerStats.ApplyAttackSpeedModifier(multiplier, duration);

            Debug.Log($"Twirling Silver: Applied {atkSpeedBonus * 100}% attack speed buff (multiplier: {multiplier}x) for {duration}s");
            Debug.Log($"Twirling Silver: New attack speed = {ownerStats.AttackSpeed}");

            // Spawn visual effect
            if (buffEffectPrefab != null)
            {
                activeBuffEffect = Instantiate(buffEffectPrefab, owner.transform);
            }
            else
            {
                // Create a subtle glow effect instead of orbiting balls
                CreateBuffGlowEffect();
            }
        }

        private void DeactivateBuff()
        {
            isActive = false;

            if (activeBuffEffect != null)
            {
                Destroy(activeBuffEffect);
                activeBuffEffect = null;
            }

            Debug.Log("Twirling Silver ended");
        }

        private void CreateBuffGlowEffect()
        {
            // Create a subtle ground ring to indicate buff is active
            activeBuffEffect = new GameObject("TwirlingSilverEffect");
            activeBuffEffect.transform.SetParent(owner.transform);
            activeBuffEffect.transform.localPosition = Vector3.up * 0.1f;

            // Create a flat cylinder as ground indicator
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.transform.SetParent(activeBuffEffect.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(1.5f, 0.02f, 1.5f); // Flat disc

            var renderer = ring.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            // Different colors for player vs AI to make it clear who's using the ability
            bool isPlayerControlled = owner != null && owner.IsPlayerControlled;
            if (isPlayerControlled)
            {
                renderer.material.color = new Color(0.6f, 0.8f, 1f, 0.5f); // Soft blue glow for player
                Debug.Log($"[TWIRLING SILVER VFX] Creating BLUE effect for PLAYER on {owner.gameObject.name}");
            }
            else
            {
                renderer.material.color = new Color(1f, 0.4f, 0.4f, 0.5f); // Soft red glow for AI/enemy
                Debug.Log($"[TWIRLING SILVER VFX] Creating RED effect for AI on {owner.gameObject.name}");
            }

            Destroy(ring.GetComponent<Collider>());

            // Add a pulsing effect
            var pulser = activeBuffEffect.AddComponent<PulseEffect>();
            pulser.pulseSpeed = 3f;
            pulser.minScale = 1.3f;
            pulser.maxScale = 1.7f;
        }

        public override void OnBasicAttackHit()
        {
            if (!isActive) return;

            // Reduce cooldowns of other abilities
            var abilities = owner.GetComponentsInChildren<AbilityBase>();
            foreach (var ability in abilities)
            {
                if (ability != this && ability.AbilityIndex != abilityIndex)
                {
                    ability.ReduceCooldown(cooldownReductionPerHit);
                }
            }

            Debug.Log($"Twirling Silver: Reduced other cooldowns by {cooldownReductionPerHit}s");
        }
    }

    /// <summary>
    /// Pulsing scale effect for buff indicators
    /// </summary>
    public class PulseEffect : MonoBehaviour
    {
        public float pulseSpeed = 2f;
        public float minScale = 0.9f;
        public float maxScale = 1.1f;

        private Vector3 baseScale;

        private void Start()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0-1 range
            float scale = Mathf.Lerp(minScale, maxScale, pulse);
            transform.localScale = new Vector3(baseScale.x * scale, baseScale.y, baseScale.z * scale);
        }
    }
}
