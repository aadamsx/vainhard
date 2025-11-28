using UnityEngine;
using System.Collections.Generic;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Map
{
    /// <summary>
    /// Jungle camp containing one or more monsters.
    /// Respawns after being cleared.
    /// </summary>
    public class JungleCamp : MonoBehaviour
    {
        [Header("Camp Settings")]
        [SerializeField] private CampType campType = CampType.Small;
        [SerializeField] private float respawnTime = 60f;
        [SerializeField] private int goldReward = 30;
        [SerializeField] private float expReward = 25f;

        [Header("Buff (Large camps only)")]
        [SerializeField] private BuffType buffType = BuffType.None;
        [SerializeField] private float buffDuration = 60f;
        [SerializeField] private float buffValue = 0.1f; // 10% bonus

        public enum CampType
        {
            Small,  // 2 small monsters
            Medium, // 1 medium monster
            Large   // 1 large monster + buff
        }

        public enum BuffType
        {
            None,
            AttackSpeed,
            CrystalPower,
            WeaponPower
        }

        // State
        private List<JungleMonster> monsters = new List<JungleMonster>();
        private bool isCleared;
        private float respawnTimer;
        private GameObject lastKiller;

        private void Start()
        {
            SpawnMonsters();
        }

        private void Update()
        {
            if (isCleared)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0)
                {
                    SpawnMonsters();
                }
            }
            else
            {
                // Check if all monsters dead
                CheckCleared();
            }
        }

        private void SpawnMonsters()
        {
            isCleared = false;
            monsters.Clear();

            switch (campType)
            {
                case CampType.Small:
                    SpawnMonster(200f, 20f, new Vector3(-0.5f, 0, 0));
                    SpawnMonster(200f, 20f, new Vector3(0.5f, 0, 0));
                    break;

                case CampType.Medium:
                    SpawnMonster(600f, 40f, Vector3.zero);
                    break;

                case CampType.Large:
                    SpawnMonster(1000f, 60f, Vector3.zero, true);
                    break;
            }
        }

        private void SpawnMonster(float health, float damage, Vector3 offset, bool isBoss = false)
        {
            var monsterObj = new GameObject($"JungleMonster_{monsters.Count}");
            monsterObj.transform.SetParent(transform);
            monsterObj.transform.localPosition = offset;

            // Visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(monsterObj.transform);
            visual.transform.localPosition = Vector3.up * 0.5f;

            float scale = isBoss ? 1.5f : (campType == CampType.Medium ? 1f : 0.6f);
            visual.transform.localScale = Vector3.one * scale;

            // Color
            var renderer = visual.GetComponent<Renderer>();
            renderer.material.color = isBoss ? new Color(0.6f, 0.2f, 0.6f) : // Purple for boss
                campType == CampType.Medium ? new Color(0.4f, 0.4f, 0.2f) : // Yellow-brown
                new Color(0.3f, 0.3f, 0.3f); // Grey for small

            // Add components
            var monster = monsterObj.AddComponent<JungleMonster>();
            var stats = monsterObj.AddComponent<CharacterStats>();
            var targetable = monsterObj.AddComponent<Targetable>();

            // Configure via monster component
            monster.Initialize(this, health, damage, isBoss);

            // Add collider
            var collider = monsterObj.AddComponent<SphereCollider>();
            collider.radius = scale * 0.5f;
            collider.center = Vector3.up * 0.5f;

            monsters.Add(monster);

            // Health bar
            var healthBar = new GameObject("HealthBar").AddComponent<UI.WorldHealthBar>();
            healthBar.Initialize(monsterObj.transform, true);
        }

        private void CheckCleared()
        {
            foreach (var monster in monsters)
            {
                if (monster != null && monster.IsAlive)
                {
                    return;
                }
            }

            // All dead
            OnCampCleared();
        }

        private void OnCampCleared()
        {
            isCleared = true;
            respawnTimer = respawnTime;

            // Award rewards to killer
            if (lastKiller != null)
            {
                var heroStats = lastKiller.GetComponent<CharacterStats>();
                if (heroStats != null)
                {
                    heroStats.AddGold(goldReward);
                    heroStats.AddExperience(expReward);

                    // Apply buff if large camp
                    if (campType == CampType.Large && buffType != BuffType.None)
                    {
                        ApplyBuff(heroStats);
                    }
                }
            }
        }

        private void ApplyBuff(CharacterStats stats)
        {
            switch (buffType)
            {
                case BuffType.AttackSpeed:
                    stats.ApplyModifier("jungle_buff", CharacterStats.StatType.AttackSpeed, 1f + buffValue, buffDuration);
                    break;
                case BuffType.CrystalPower:
                    stats.ApplyModifier("jungle_buff", CharacterStats.StatType.CrystalPower, 1f + buffValue, buffDuration);
                    break;
                case BuffType.WeaponPower:
                    stats.ApplyModifier("jungle_buff", CharacterStats.StatType.WeaponPower, 1f + buffValue, buffDuration);
                    break;
            }
        }

        public void ReportKill(GameObject killer)
        {
            lastKiller = killer;
        }

        public int GoldReward => goldReward;
        public float ExpReward => expReward;

        public void Configure(CampType type, BuffType buff = BuffType.None)
        {
            campType = type;
            buffType = buff;

            // Set rewards based on type
            switch (type)
            {
                case CampType.Small:
                    goldReward = 30;
                    expReward = 25f;
                    respawnTime = 50f;
                    break;
                case CampType.Medium:
                    goldReward = 55;
                    expReward = 45f;
                    respawnTime = 60f;
                    break;
                case CampType.Large:
                    goldReward = 100;
                    expReward = 80f;
                    respawnTime = 90f;
                    buffDuration = 60f;
                    buffValue = 0.15f; // 15% bonus
                    break;
            }
        }
    }

    /// <summary>
    /// Individual jungle monster in a camp.
    /// </summary>
    public class JungleMonster : MonoBehaviour
    {
        private JungleCamp camp;
        private CharacterStats stats;
        private float attackDamage;
        private float attackRange = 2f;
        private float attackCooldown = 1f;
        private float aggroRange = 6f;
        private bool isBoss;

        private GameObject currentTarget;
        private Vector3 homePosition;
        private float lastAttackTime;
        private float leashRange = 10f;

        public bool IsAlive => stats != null && stats.IsAlive;

        public void Initialize(JungleCamp camp, float health, float damage, bool isBoss)
        {
            this.camp = camp;
            this.attackDamage = damage;
            this.isBoss = isBoss;
            this.homePosition = transform.position;

            stats = GetComponent<CharacterStats>();
            stats.SetBaseStats(health, damage, 0f);
            stats.OnDeath.AddListener(OnDeath);
            stats.OnDeathWithKiller.AddListener(OnDeathWithKiller);
        }

        private void Update()
        {
            if (!IsAlive) return;

            UpdateTarget();
            UpdateBehavior();
        }

        private void UpdateTarget()
        {
            // Check leash
            float distanceFromHome = Vector3.Distance(transform.position, homePosition);
            if (distanceFromHome > leashRange)
            {
                // Reset and return home
                currentTarget = null;
                transform.position = homePosition;
                stats.FullHeal();
                return;
            }

            // Check current target validity
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

            // Find new target
            if (currentTarget == null)
            {
                currentTarget = FindTarget();
            }
        }

        private GameObject FindTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, aggroRange);

            foreach (var col in colliders)
            {
                var hero = col.GetComponent<Characters.HeroController>();
                if (hero != null)
                {
                    var heroStats = hero.GetComponent<CharacterStats>();
                    if (heroStats != null && heroStats.IsAlive)
                    {
                        return col.gameObject;
                    }
                }
            }

            return null;
        }

        private void UpdateBehavior()
        {
            if (currentTarget == null)
            {
                // Return home slowly
                if (Vector3.Distance(transform.position, homePosition) > 0.5f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, homePosition, 2f * Time.deltaTime);
                }
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distanceToTarget <= attackRange)
            {
                // Attack
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                // Move towards target
                Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
                transform.position += direction * 3f * Time.deltaTime;

                // Face target
                direction.y = 0;
                if (direction.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
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
        }

        private void OnDeath()
        {
            // Disable
            GetComponent<Collider>().enabled = false;

            // Destroy after brief delay
            Destroy(gameObject, 0.5f);
        }

        private void OnDeathWithKiller(GameObject killer)
        {
            // Report to camp for gold/XP rewards
            if (camp != null && killer != null)
            {
                camp.ReportKill(killer);
            }

            // Notify the killer's HeroController (for Double Down passive)
            if (killer != null)
            {
                var killerHero = killer.GetComponent<Characters.HeroController>();
                if (killerHero != null)
                {
                    killerHero.OnKilledTarget(gameObject);
                }
            }
        }
    }
}
