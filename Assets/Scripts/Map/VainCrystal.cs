using UnityEngine;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Map
{
    /// <summary>
    /// The main objective - destroy the enemy Vain Crystal to win.
    /// Protected by turrets.
    /// </summary>
    [RequireComponent(typeof(CharacterStats))]
    public class VainCrystal : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameManager.Team team = GameManager.Team.Blue;
        [SerializeField] private Turret[] protectingTurrets;

        [Header("Visual")]
        [SerializeField] private GameObject crystalModel;
        [SerializeField] private GameObject shieldEffect;
        [SerializeField] private ParticleSystem glowParticles;

        [Header("Healing Zone")]
        [SerializeField] private float healRadius = 5f;
        [SerializeField] private float healRate = 50f; // Per second

        // Components
        private CharacterStats stats;
        private Targetable targetable;

        // State
        private bool isVulnerable;
        private bool isDestroyed;

        public bool IsDestroyed => isDestroyed;
        public bool IsVulnerable => isVulnerable;
        public GameManager.Team Team => team;

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();
            targetable = GetComponent<Targetable>();
        }

        private void Start()
        {
            stats.OnDeath.AddListener(OnCrystalDestroyed);
            UpdateVulnerability();
        }

        /// <summary>
        /// Initialize the crystal with team and protecting turrets
        /// </summary>
        public void Initialize(GameManager.Team crystalTeam, Turret[] turrets)
        {
            team = crystalTeam;
            protectingTurrets = turrets;

            if (targetable != null)
            {
                targetable.SetTeam(team);
            }

            Debug.Log($"{gameObject.name} initialized with {(turrets != null ? turrets.Length : 0)} protecting turrets");
        }

        private void Update()
        {
            if (isDestroyed) return;

            UpdateVulnerability();
            UpdateHealingZone();
            UpdateVisuals();
        }

        private void UpdateVulnerability()
        {
            // Check if all protecting turrets are destroyed
            bool allTurretsDown = true;
            if (protectingTurrets != null && protectingTurrets.Length > 0)
            {
                foreach (var turret in protectingTurrets)
                {
                    if (turret != null && !turret.IsDestroyed)
                    {
                        allTurretsDown = false;
                        break;
                    }
                }
            }

            bool wasVulnerable = isVulnerable;
            isVulnerable = allTurretsDown;

            // Log state change
            if (isVulnerable && !wasVulnerable)
            {
                Debug.Log($"{gameObject.name} is now VULNERABLE - all turrets destroyed!");
            }

            // Update shield effect
            if (shieldEffect != null)
            {
                shieldEffect.SetActive(!isVulnerable);
            }

            // If not vulnerable, heal any damage taken (makes it effectively invulnerable)
            // This is a workaround since we can't intercept CharacterStats.TakeDamage
            if (!isVulnerable && stats != null)
            {
                stats.FullHeal();
            }
        }

        private void UpdateHealingZone()
        {
            // Heal allied heroes in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, healRadius);

            foreach (var col in colliders)
            {
                var heroController = col.GetComponent<HeroController>();
                if (heroController == null) continue;
                if (heroController.Team != team) continue;

                var heroStats = heroController.Stats;
                if (heroStats != null && heroStats.IsAlive)
                {
                    heroStats.Heal(healRate * Time.deltaTime, false);
                    heroStats.RestoreEnergy(healRate * 0.5f * Time.deltaTime, false);
                }
            }
        }

        private void UpdateVisuals()
        {
            // Pulse effect or rotation
            if (crystalModel != null)
            {
                crystalModel.transform.Rotate(Vector3.up, 20f * Time.deltaTime);
            }

            // Color based on vulnerability
            if (glowParticles != null)
            {
                var main = glowParticles.main;
                main.startColor = isVulnerable ?
                    (team == GameManager.Team.Blue ? Color.blue : Color.red) :
                    Color.white;
            }
        }

        private void OnCrystalDestroyed()
        {
            isDestroyed = true;

            Debug.Log($"{gameObject.name} has been DESTROYED!");

            // Determine winner (opposite team wins)
            GameManager.Team winner = team == GameManager.Team.Blue ?
                GameManager.Team.Red : GameManager.Team.Blue;

            // Notify game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(winner);
            }

            // Disable collider
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Visual destruction sequence
            StartCoroutine(DestructionSequence());
        }

        private System.Collections.IEnumerator DestructionSequence()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            float elapsed = 0f;
            float duration = 2f;

            // Flash and explode effect
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Scale up then disappear
                float scale = 1f + t * 0.5f;
                transform.localScale = Vector3.one * scale;

                // Fade
                foreach (var r in renderers)
                {
                    Color c = r.material.color;
                    c.a = 1f - t;
                    r.material.color = c;
                }

                yield return null;
            }

            // Hide
            foreach (var r in renderers)
            {
                r.enabled = false;
            }
        }

        /// <summary>
        /// Override damage to check vulnerability
        /// </summary>
        public void TakeDamage(float amount, CharacterStats.DamageType type, GameObject source)
        {
            if (!isVulnerable)
            {
                // Show "invulnerable" feedback
                return;
            }

            stats.TakeDamage(amount, type, source);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, healRadius);
        }
    }
}
