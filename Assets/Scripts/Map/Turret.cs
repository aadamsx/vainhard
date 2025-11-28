using UnityEngine;
using System.Collections.Generic;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Map
{
    /// <summary>
    /// Defensive turret that attacks enemies in range.
    /// Prioritizes minions, but targets heroes if they attack allies.
    /// </summary>
    [RequireComponent(typeof(CharacterStats))]
    public class Turret : MonoBehaviour
    {
        [Header("Settings - Vainglory values")]
        [SerializeField] private GameManager.Team team = GameManager.Team.Blue;
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float attackDamage = 160f; // Vainglory turret base damage
        [SerializeField] private float attackCooldown = 1.25f; // Vainglory turret attack speed
        [SerializeField] private bool isOuterTurret = true;
        [SerializeField] private float damageStackPercent = 0.45f; // 45% more per consecutive hit
        [SerializeField] private int maxDamageStacks = 6;

        // Damage stacking on same target
        private int currentDamageStacks = 0;
        private GameObject lastTarget;

        [Header("Visual")]
        [SerializeField] private Transform turretHead;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private LineRenderer rangeIndicator;

        [Header("Audio")]
        [SerializeField] private AudioClip fireSound;

        // Components
        private CharacterStats stats;
        private Targetable targetable;
        private AudioSource audioSource;

        // State
        private GameObject currentTarget;
        private float lastAttackTime;
        private List<GameObject> threatsToAllies = new List<GameObject>();
        private bool isDestroyed;

        // Outer turret reference (inner turret needs this)
        public Turret outerTurret;

        public bool IsDestroyed => isDestroyed;
        public GameManager.Team Team => team;

        /// <summary>
        /// Set the turret's team (used during runtime creation)
        /// </summary>
        public void SetTeam(GameManager.Team newTeam)
        {
            team = newTeam;
            if (targetable != null)
            {
                targetable.SetTeam(team);
            }
        }

        /// <summary>
        /// Set if this is an outer turret (inner turrets need outer destroyed first)
        /// </summary>
        public void SetIsOuterTurret(bool isOuter)
        {
            isOuterTurret = isOuter;
        }

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();
            targetable = GetComponent<Targetable>();
            audioSource = GetComponent<AudioSource>();

            if (targetable != null)
            {
                targetable.SetTeam(team);
            }
        }

        private void Start()
        {
            stats.OnDeath.AddListener(OnTurretDestroyed);
            CreateRangeIndicator();
        }

        private void Update()
        {
            if (isDestroyed) return;

            // Check if we can attack (inner turret needs outer to be destroyed first)
            if (!isOuterTurret && outerTurret != null && !outerTurret.IsDestroyed)
            {
                // Inner turret is invulnerable while outer stands
                return;
            }

            UpdateTarget();
            UpdateAttack();
            UpdateVisuals();
        }

        private void UpdateTarget()
        {
            // Clear invalid target
            if (currentTarget != null)
            {
                var targetStats = currentTarget.GetComponent<CharacterStats>();
                if (targetStats == null || !targetStats.IsAlive)
                {
                    currentTarget = null;
                }
                else
                {
                    float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                    if (distance > attackRange)
                    {
                        currentTarget = null;
                    }
                }
            }

            // Find new target if needed
            if (currentTarget == null)
            {
                currentTarget = FindBestTarget();
            }
        }

        private GameObject FindBestTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);

            GameObject bestMinion = null;
            GameObject bestHero = null;
            float closestMinionDist = float.MaxValue;
            float closestHeroDist = float.MaxValue;

            foreach (var col in colliders)
            {
                var targetable = col.GetComponent<Targetable>();
                if (targetable == null) continue;
                if (!targetable.CanBeTargetedBy(team)) continue;

                var targetStats = col.GetComponent<CharacterStats>();
                if (targetStats == null || !targetStats.IsAlive) continue;

                float distance = Vector3.Distance(transform.position, col.transform.position);

                // Check if this is a hero that attacked our allies
                var heroController = col.GetComponent<HeroController>();
                if (heroController != null)
                {
                    if (threatsToAllies.Contains(col.gameObject))
                    {
                        if (distance < closestHeroDist)
                        {
                            closestHeroDist = distance;
                            bestHero = col.gameObject;
                        }
                    }
                }
                else
                {
                    // Minion or monster
                    if (distance < closestMinionDist)
                    {
                        closestMinionDist = distance;
                        bestMinion = col.gameObject;
                    }
                }
            }

            // Prioritize: threatening heroes > minions > other heroes
            if (bestHero != null && threatsToAllies.Contains(bestHero))
            {
                return bestHero;
            }
            if (bestMinion != null)
            {
                return bestMinion;
            }

            return bestHero;
        }

        private void UpdateAttack()
        {
            if (currentTarget == null) return;
            if (Time.time < lastAttackTime + attackCooldown) return;

            // Fire at target
            Fire();
            lastAttackTime = Time.time;
        }

        private void Fire()
        {
            if (currentTarget == null) return;

            // Track damage stacking on same target
            if (currentTarget == lastTarget)
            {
                currentDamageStacks = Mathf.Min(currentDamageStacks + 1, maxDamageStacks);
            }
            else
            {
                currentDamageStacks = 0;
                lastTarget = currentTarget;
            }

            // Calculate damage with stacking bonus
            float stackMultiplier = 1f + (damageStackPercent * currentDamageStacks);
            float finalDamage = attackDamage * stackMultiplier;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 3f;

            // Create projectile
            GameObject projObj;
            if (projectilePrefab != null)
            {
                projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Placeholder projectile
                projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.position = spawnPos;
                projObj.transform.localScale = Vector3.one * 0.4f;
                projObj.GetComponent<Renderer>().material.color = team == GameManager.Team.Blue ? Color.cyan : Color.red;

                var collider = projObj.GetComponent<Collider>();
                collider.isTrigger = true;

                var rb = projObj.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            var projectile = projObj.AddComponent<Projectile>();
            projectile.InitializeHoming(currentTarget.transform, 15f, finalDamage,
                CharacterStats.DamageType.Physical, gameObject, false, null);

            // Play sound
            if (audioSource != null && fireSound != null)
            {
                audioSource.PlayOneShot(fireSound);
            }
        }

        private void UpdateVisuals()
        {
            // Rotate turret head to face target
            if (turretHead != null && currentTarget != null)
            {
                Vector3 direction = (currentTarget.transform.position - turretHead.position).normalized;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    turretHead.rotation = Quaternion.Slerp(turretHead.rotation, targetRotation, 10f * Time.deltaTime);
                }
            }
        }

        private void CreateRangeIndicator()
        {
            if (rangeIndicator != null)
            {
                // Draw circle
                int segments = 64;
                rangeIndicator.positionCount = segments + 1;
                rangeIndicator.loop = true;

                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * 2f * Mathf.PI / segments;
                    float x = Mathf.Cos(angle) * attackRange;
                    float z = Mathf.Sin(angle) * attackRange;
                    rangeIndicator.SetPosition(i, new Vector3(x, 0.1f, z));
                }
            }
        }

        /// <summary>
        /// Called when an ally is attacked by a hero
        /// </summary>
        public void ReportThreat(GameObject attacker)
        {
            if (!threatsToAllies.Contains(attacker))
            {
                threatsToAllies.Add(attacker);

                // Clear threat after leaving range or timeout
                StartCoroutine(ClearThreatAfterDelay(attacker, 3f));
            }
        }

        private System.Collections.IEnumerator ClearThreatAfterDelay(GameObject threat, float delay)
        {
            yield return new WaitForSeconds(delay);
            threatsToAllies.Remove(threat);
        }

        private void OnTurretDestroyed()
        {
            isDestroyed = true;
            currentTarget = null;

            Debug.Log($"{gameObject.name} has been destroyed!");

            // Disable targeting
            if (targetable != null)
            {
                Destroy(targetable);
            }

            // Disable collider so it can't be targeted anymore
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Visual feedback - fade out and collapse
            StartCoroutine(DestructionSequence());
        }

        private System.Collections.IEnumerator DestructionSequence()
        {
            // Get all renderers
            var renderers = GetComponentsInChildren<Renderer>();
            float elapsed = 0f;
            float duration = 1f;

            // Store original colors
            var originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].material.color;
            }

            // Fade out and sink
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Fade alpha
                for (int i = 0; i < renderers.Length; i++)
                {
                    Color c = originalColors[i];
                    c.a = 1f - t;
                    renderers[i].material.color = c;
                }

                // Sink into ground
                transform.position += Vector3.down * Time.deltaTime * 2f;

                yield return null;
            }

            // Disable visuals
            foreach (var r in renderers)
            {
                r.enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
