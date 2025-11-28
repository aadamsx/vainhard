using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.Combat
{
    /// <summary>
    /// Taka's B ability - Go invisible, heal over time, and gain movement speed.
    /// Attacking or taking damage breaks stealth.
    /// </summary>
    public class Kaku : AbilityBase
    {
        [Header("Kaku - Vainglory Stats")]
        [SerializeField] private float stealthDuration = 3f;

        // Level-based stats
        private float[] healPerSeconds = { 40f, 55f, 70f, 85f, 100f };
        private float[] durations = { 3f, 3f, 3f, 3f, 4f };
        private float[] cooldowns = { 18f, 17f, 16f, 15f, 13f };
        private float[] energyCosts = { 80f, 85f, 90f, 95f, 100f };

        protected override void Awake()
        {
            base.Awake();

            abilityName = "Kaku";
            abilityIndex = 1;
            targetingType = TargetingType.Instant;
            range = 0f;
            baseDamage = 0f;
            energyCost = energyCosts[0];
            cooldown = cooldowns[0];
        }

        protected override void OnLevelUp()
        {
            int lvl = Mathf.Clamp(currentLevel - 1, 0, 4);
            energyCost = energyCosts[lvl];
            cooldown = cooldowns[lvl];
            stealthDuration = durations[lvl];
        }

        private float GetHealPerSecond()
        {
            return healPerSeconds[Mathf.Clamp(currentLevel - 1, 0, 4)];
        }

        protected override void Execute(Vector3? targetPosition, GameObject targetUnit)
        {
            // Enter stealth
            var takaHero = owner.GetComponent<TakaHero>();
            if (takaHero != null)
            {
                takaHero.EnterStealth(stealthDuration, GetHealPerSecond());
            }

            // Apply move speed bonus during stealth
            ownerStats.ApplyModifier("kaku_speed", CharacterStats.StatType.MoveSpeed, 1.2f, stealthDuration);

            // Grant Ki stack
            if (takaHero != null)
            {
                takaHero.OnAbilityHit();
            }

            Debug.Log($"Kaku activated: {stealthDuration}s stealth, healing {GetHealPerSecond()} HP/s");
        }

        public override TargetingIndicatorData GetTargetingIndicator()
        {
            return new TargetingIndicatorData
            {
                type = TargetingType.Instant,
                range = 0,
                radius = 0
            };
        }
    }
}
