using UnityEngine;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Characters
{
    /// <summary>
    /// Ringo-specific hero logic, including Double Down passive.
    /// In Vainglory, Double Down gives Ringo a guaranteed crit on his next
    /// basic attack after killing any enemy (hero, minion, jungle monster).
    /// </summary>
    public class RingoHero : MonoBehaviour
    {
        [Header("Double Down Passive - Vainglory")]
        [SerializeField] private float critDamageMultiplier = 1.8f; // 80% bonus = 180% total damage
        [SerializeField] private float passiveDuration = 6f; // Expires after 6 seconds if not used

        [Header("Visual")]
        [SerializeField] private GameObject passiveReadyEffect;

        private bool passiveReady = false;
        private float passiveExpireTime = 0f;

        private CharacterStats ownerStats;
        private HeroController heroController;

        public bool IsPassiveReady => passiveReady && Time.time < passiveExpireTime;

        private void Awake()
        {
            ownerStats = GetComponent<CharacterStats>();
            heroController = GetComponent<HeroController>();
        }

        private void Start()
        {
            // Subscribe to kill events to trigger passive
            if (ownerStats != null)
            {
                // We need to listen for when THIS hero kills something
                // This is handled through the damage source tracking
            }
        }

        private void Update()
        {
            // Check if passive expired
            if (passiveReady && Time.time >= passiveExpireTime)
            {
                passiveReady = false;
                if (passiveReadyEffect != null)
                {
                    passiveReadyEffect.SetActive(false);
                }
                Debug.Log("Double Down expired");
            }
        }

        /// <summary>
        /// Called when Ringo kills any enemy (minion, monster, hero).
        /// Activates the Double Down passive for the next basic attack.
        /// </summary>
        public void OnKill(GameObject victim)
        {
            passiveReady = true;
            passiveExpireTime = Time.time + passiveDuration;

            if (passiveReadyEffect != null)
            {
                passiveReadyEffect.SetActive(true);
            }

            Debug.Log($"Double Down activated! Next attack will crit. (Killed: {victim.name})");
        }

        /// <summary>
        /// Called by HeroController when attacking. Returns modified damage.
        /// Consumes the passive if active.
        /// </summary>
        public float ModifyAttackDamage(float baseDamage)
        {
            if (IsPassiveReady)
            {
                passiveReady = false;

                if (passiveReadyEffect != null)
                {
                    passiveReadyEffect.SetActive(false);
                }

                float critDamage = baseDamage * critDamageMultiplier;
                Debug.Log($"Double Down CRIT! {baseDamage:F0} -> {critDamage:F0}");
                return critDamage;
            }

            return baseDamage;
        }

        /// <summary>
        /// Reset passive state (for respawn)
        /// </summary>
        public void ResetPassive()
        {
            passiveReady = false;
            passiveExpireTime = 0f;
        }
    }
}
