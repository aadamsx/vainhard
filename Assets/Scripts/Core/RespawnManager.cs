using UnityEngine;
using System.Collections.Generic;
using VaingloryMoba.Characters;
using VaingloryMoba.Map;

namespace VaingloryMoba.Core
{
    /// <summary>
    /// Manages hero death and respawn timers.
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        public static RespawnManager Instance { get; private set; }

        [Header("Settings - Vainglory Style")]
        // Vainglory respawn: starts at 10s, scales with game time and level
        // Early game (0-5 min): 10-20s
        // Mid game (5-15 min): 20-40s
        // Late game (15+ min): 40-60s
        [SerializeField] private float baseRespawnTime = 10f;
        [SerializeField] private float respawnTimePerLevel = 2.5f;
        [SerializeField] private float respawnTimePerMinute = 1.5f; // Additional scaling with game time
        [SerializeField] private float maxRespawnTime = 60f;

        [Header("References")]
        [SerializeField] private Transform blueSpawnPoint;
        [SerializeField] private Transform redSpawnPoint;

        // Respawn queue
        private Dictionary<GameObject, RespawnData> pendingRespawns = new Dictionary<GameObject, RespawnData>();

        private class RespawnData
        {
            public float respawnTime;
            public GameManager.Team team;
            public Vector3 spawnPosition;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Find spawn points from map generator if not assigned
            var mapGen = FindObjectOfType<MapGenerator>();
            if (mapGen != null)
            {
                if (blueSpawnPoint == null) blueSpawnPoint = mapGen.BlueSpawnPoint;
                if (redSpawnPoint == null) redSpawnPoint = mapGen.RedSpawnPoint;
            }
        }

        private void Update()
        {
            // Process respawn queue
            var toRespawn = new List<GameObject>();

            foreach (var kvp in pendingRespawns)
            {
                if (Time.time >= kvp.Value.respawnTime)
                {
                    toRespawn.Add(kvp.Key);
                }
            }

            foreach (var hero in toRespawn)
            {
                RespawnHero(hero);
            }
        }

        /// <summary>
        /// Register a hero death for respawn
        /// </summary>
        public void RegisterDeath(GameObject hero, GameManager.Team team)
        {
            if (pendingRespawns.ContainsKey(hero))
            {
                return; // Already pending
            }

            var stats = hero.GetComponent<CharacterStats>();
            int level = stats != null ? stats.Level : 1;

            // Calculate respawn time based on level and game time
            float gameMinutes = Time.time / 60f;
            float levelBonus = respawnTimePerLevel * (level - 1);
            float timeBonus = respawnTimePerMinute * gameMinutes;
            float respawnDelay = Mathf.Min(baseRespawnTime + levelBonus + timeBonus, maxRespawnTime);

            Vector3 spawnPos = team == GameManager.Team.Blue ?
                (blueSpawnPoint != null ? blueSpawnPoint.position : new Vector3(40, 0, 5)) :
                (redSpawnPoint != null ? redSpawnPoint.position : new Vector3(40, 0, 55));

            pendingRespawns[hero] = new RespawnData
            {
                respawnTime = Time.time + respawnDelay,
                team = team,
                spawnPosition = spawnPos
            };

            // Hide the hero
            hero.SetActive(false);

            Debug.Log($"{hero.name} will respawn in {respawnDelay:F1}s");
        }

        private void RespawnHero(GameObject hero)
        {
            if (!pendingRespawns.TryGetValue(hero, out RespawnData data))
            {
                return;
            }

            pendingRespawns.Remove(hero);

            // Move and reactivate
            hero.transform.position = data.spawnPosition;
            hero.SetActive(true);

            // Restore health and energy
            var stats = hero.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.FullHeal();
            }

            var controller = hero.GetComponent<HeroController>();
            if (controller != null)
            {
                controller.Respawn(data.spawnPosition);
            }

            Debug.Log($"{hero.name} has respawned!");
        }

        /// <summary>
        /// Get remaining respawn time for a hero
        /// </summary>
        public float GetRespawnTime(GameObject hero)
        {
            if (pendingRespawns.TryGetValue(hero, out RespawnData data))
            {
                return Mathf.Max(0, data.respawnTime - Time.time);
            }
            return 0;
        }

        /// <summary>
        /// Check if a hero is pending respawn
        /// </summary>
        public bool IsPendingRespawn(GameObject hero)
        {
            return pendingRespawns.ContainsKey(hero);
        }
    }
}
