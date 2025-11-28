using UnityEngine;
using UnityEngine.AI;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Map
{
    /// <summary>
    /// Lane minion that walks down the lane and attacks enemies.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CharacterStats))]
    public class Minion : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameManager.Team team;
        [SerializeField] private MinionType minionType = MinionType.Melee;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float aggroRange = 5f;
        [SerializeField] private int goldReward = 22; // Melee default, ranged is 17
        [SerializeField] private float expReward = 30f; // Melee default, ranged is 25

        public enum MinionType
        {
            Melee,
            Ranged
        }

        // Components
        private NavMeshAgent agent;
        private CharacterStats stats;
        private Targetable targetable;

        // State
        private Transform targetDestination;
        private GameObject currentTarget;
        private float lastAttackTime;
        private Vector3[] waypoints;
        private int currentWaypointIndex;

        public GameManager.Team Team => team;
        public int GoldReward => goldReward;
        public float ExpReward => expReward;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            stats = GetComponent<CharacterStats>();
            targetable = GetComponent<Targetable>();

            if (targetable != null)
            {
                targetable.SetTeam(team);
            }

            // Configure agent
            agent.speed = 3f;
            agent.stoppingDistance = minionType == MinionType.Melee ? 1f : attackRange - 0.5f;
        }

        private void Start()
        {
            stats.OnDeath.AddListener(OnDeath);
            stats.OnDeathWithKiller.AddListener(OnDeathWithKiller);

            // Create health bar
            var healthBar = new GameObject("HealthBar").AddComponent<UI.WorldHealthBar>();
            healthBar.Initialize(transform, team == GameManager.Team.Red);
        }

        public void Initialize(GameManager.Team team, Vector3[] waypoints)
        {
            this.team = team;
            this.waypoints = waypoints;
            this.currentWaypointIndex = 0;

            if (targetable != null)
            {
                targetable.SetTeam(team);
            }

            // Set minion stats - Vainglory values
            // Melee: tankier, less damage. Ranged: squishier, more damage
            float health = minionType == MinionType.Melee ? 450f : 280f;
            float damage = minionType == MinionType.Melee ? 12f : 23f;
            stats.SetBaseStats(health, damage, 0f);

            // Set gold/XP rewards based on type
            goldReward = minionType == MinionType.Melee ? 22 : 17;
            expReward = minionType == MinionType.Melee ? 30f : 25f;
            attackRange = minionType == MinionType.Melee ? 1.5f : 5f;

            // Set visual color based on team
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = team == GameManager.Team.Blue ?
                    new Color(0.3f, 0.5f, 0.9f) : new Color(0.9f, 0.3f, 0.3f);
            }

            // Start moving to first waypoint using direct movement (NavMesh unreliable)
            useDirectMovement = true;
        }

        // Direct movement fallback
        private bool useDirectMovement = true;
        private float moveSpeed = 3f;


        private void Update()
        {
            if (!stats.IsAlive) return;

            UpdateTarget();
            UpdateBehavior();
        }

        private void UpdateTarget()
        {
            // Check if current target is still valid
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
                    if (distance > aggroRange * 1.5f)
                    {
                        currentTarget = null;
                    }
                }
            }

            // Find new target if needed
            if (currentTarget == null)
            {
                currentTarget = FindTarget();
            }
        }

        private GameObject FindTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, aggroRange);

            GameObject closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                var targetable = col.GetComponent<Targetable>();
                if (targetable == null) continue;
                if (!targetable.CanBeTargetedBy(team)) continue;

                var targetStats = col.GetComponent<CharacterStats>();
                if (targetStats == null || !targetStats.IsAlive) continue;

                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = col.gameObject;
                }
            }

            return closestEnemy;
        }

        private void UpdateBehavior()
        {
            if (currentTarget != null)
            {
                // Combat behavior
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (distanceToTarget <= attackRange)
                {
                    // In range - stop and attack
                    // Face target
                    Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
                    direction.y = 0;
                    if (direction.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.LookRotation(direction);
                    }

                    // Attack
                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        Attack();
                    }
                }
                else
                {
                    // Move towards target using direct movement
                    Vector3 toTarget = currentTarget.transform.position - transform.position;
                    toTarget.y = 0;
                    Vector3 moveDir = toTarget.normalized;
                    transform.position += moveDir * moveSpeed * Time.deltaTime;

                    if (moveDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.LookRotation(moveDir);
                    }
                }
            }
            else
            {
                // No target - walk towards destination
                WalkLane();
            }
        }

        private void WalkLane()
        {
            if (waypoints == null || waypoints.Length == 0) return;
            if (currentWaypointIndex >= waypoints.Length) return;

            Vector3 target = waypoints[currentWaypointIndex];
            Vector3 toTarget = target - transform.position;
            toTarget.y = 0;

            // Check if reached current waypoint
            if (toTarget.magnitude < 1f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length) return;
                target = waypoints[currentWaypointIndex];
                toTarget = target - transform.position;
                toTarget.y = 0;
            }

            // Direct movement
            if (useDirectMovement)
            {
                Vector3 moveDir = toTarget.normalized;
                transform.position += moveDir * moveSpeed * Time.deltaTime;

                // Face movement direction
                if (moveDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(moveDir);
                }
            }
            else
            {
                // NavMesh fallback
                if (!agent.hasPath || agent.remainingDistance < 1f)
                {
                    agent.SetDestination(target);
                }
            }
        }

        private void Attack()
        {
            if (currentTarget == null) return;

            lastAttackTime = Time.time;

            var targetStats = currentTarget.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(attackDamage, CharacterStats.DamageType.Physical, gameObject);
            }

            // TODO: Visual/audio feedback
        }

        private void OnDeath()
        {
            // Disable
            agent.enabled = false;
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Fade out and destroy
            StartCoroutine(DeathSequence());
        }

        private void OnDeathWithKiller(GameObject killer)
        {
            // Last-hit mechanic: only the killer gets gold and XP
            if (killer == null) return;

            var killerStats = killer.GetComponent<CharacterStats>();
            if (killerStats != null)
            {
                killerStats.AddGold(goldReward);
                killerStats.AddExperience(expReward);
                Debug.Log($"{killer.name} last-hit {gameObject.name} for {goldReward} gold and {expReward} XP");
            }

            // Notify the killer's HeroController (for Double Down passive)
            var killerHero = killer.GetComponent<HeroController>();
            if (killerHero != null)
            {
                killerHero.OnKilledTarget(gameObject);
            }
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            // Simple fade out
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                float elapsed = 0f;
                Color originalColor = renderer.material.color;

                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    float alpha = 1f - (elapsed / 0.5f);
                    renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    yield return null;
                }
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
