using UnityEngine;
using System.Collections.Generic;
using VaingloryMoba.Core;

namespace VaingloryMoba.Map
{
    /// <summary>
    /// Spawns minion waves at regular intervals.
    /// </summary>
    public class MinionSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float waveInterval = 30f;
        [SerializeField] private float spawnDelay = 0.8f; // Delay between each minion in wave
        [SerializeField] private int meleePerWave = 3;
        [SerializeField] private int rangedPerWave = 3; // Vainglory has 3 melee + 3 ranged

        [Header("Prefabs")]
        [SerializeField] private GameObject meleeMinionPrefab;
        [SerializeField] private GameObject rangedMinionPrefab;

        [Header("Spawn Points")]
        [SerializeField] private Transform blueSpawnPoint;
        [SerializeField] private Transform redSpawnPoint;

        [Header("Waypoints")]
        [SerializeField] private Transform[] blueWaypoints;
        [SerializeField] private Transform[] redWaypoints;

        // State
        private float nextWaveTime;
        private int waveCount;
        private MapGenerator mapGenerator;

        private void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();

            // FIXED COORDINATES matching MapGenerator:
            // Crystal Vein is in LOWER part of base (where minions spawn from)
            const float BLUE_BASE_X = 10f;
            const float RED_BASE_X = 90f;
            const float BASE_CENTER_Z = 40f;
            const float BASE_HEIGHT_Z = 40f;

            // Crystal is in lower portion of base
            float crystalZ = BASE_CENTER_Z - BASE_HEIGHT_Z * 0.25f;  // ~30

            // Minions spawn from the Crystal Vein
            if (blueSpawnPoint == null)
            {
                // Try to get from MapGenerator first
                if (mapGenerator != null && mapGenerator.BlueCrystal != null)
                {
                    blueSpawnPoint = mapGenerator.BlueCrystal.transform;
                }
                else
                {
                    var blueMinSpawn = new GameObject("BlueMinionSpawn");
                    blueMinSpawn.transform.position = new Vector3(BLUE_BASE_X, 0, crystalZ);
                    blueSpawnPoint = blueMinSpawn.transform;
                }
            }

            if (redSpawnPoint == null)
            {
                // Try to get from MapGenerator first
                if (mapGenerator != null && mapGenerator.RedCrystal != null)
                {
                    redSpawnPoint = mapGenerator.RedCrystal.transform;
                }
                else
                {
                    var redMinSpawn = new GameObject("RedMinionSpawn");
                    redMinSpawn.transform.position = new Vector3(RED_BASE_X, 0, crystalZ);
                    redSpawnPoint = redMinSpawn.transform;
                }
            }

            // Generate waypoints if not assigned
            if (blueWaypoints == null || blueWaypoints.Length == 0)
            {
                GenerateDefaultWaypoints();
            }

            // First wave spawns quickly (5 seconds)
            nextWaveTime = Time.time + 5f;
            Debug.Log("MinionSpawner: First wave in 5 seconds");
        }

        private void GenerateDefaultWaypoints()
        {
            // STRAIGHT lane from X=20 to X=80 at Z=40
            var blueWaypointList = new List<Transform>();
            var redWaypointList = new List<Transform>();

            const float LANE_START_X = 20f;
            const float LANE_END_X = 80f;
            const float LANE_Z = 40f;
            
            // OFFSET: Minions walk BELOW the turrets (Turrets are at +2.5, Minions at -2.5)
            float laneOffsetZ = -2.5f;

            // BLUE WAYPOINTS (walking from left to right along lane):
            var blueWp0 = new GameObject("BlueWaypoint_Start");
            blueWp0.transform.position = new Vector3(LANE_START_X, 0, LANE_Z + laneOffsetZ);
            blueWp0.transform.SetParent(transform);
            blueWaypointList.Add(blueWp0.transform);

            int laneWaypointCount = 10;
            for (int i = 0; i < laneWaypointCount; i++)
            {
                float t = 0.05f + (0.90f * i / (laneWaypointCount - 1));
                var wp = new GameObject($"BlueWaypoint_{i + 1}");

                Vector3 lanePos;
                if (mapGenerator != null)
                {
                    lanePos = mapGenerator.GetLanePosition(t);
                }
                else
                {
                    lanePos = GetLanePointFallback(t);
                }
                
                // Apply offset to put minions "below" turrets
                lanePos.z += laneOffsetZ;

                wp.transform.position = lanePos;
                wp.transform.SetParent(transform);
                blueWaypointList.Add(wp.transform);
            }

            // RED WAYPOINTS (walking from right to left along lane):
            var redWp0 = new GameObject("RedWaypoint_Start");
            redWp0.transform.position = new Vector3(LANE_END_X, 0, LANE_Z + laneOffsetZ);
            redWp0.transform.SetParent(transform);
            redWaypointList.Add(redWp0.transform);

            for (int i = 0; i < laneWaypointCount; i++)
            {
                float t = 0.95f - (0.90f * i / (laneWaypointCount - 1));
                var wp = new GameObject($"RedWaypoint_{i + 1}");

                Vector3 lanePos;
                if (mapGenerator != null)
                {
                    lanePos = mapGenerator.GetLanePosition(t);
                }
                else
                {
                    lanePos = GetLanePointFallback(t);
                }
                
                // Apply offset
                lanePos.z += laneOffsetZ;

                wp.transform.position = lanePos;
                wp.transform.SetParent(transform);
                redWaypointList.Add(wp.transform);
            }

            blueWaypoints = blueWaypointList.ToArray();
            redWaypoints = redWaypointList.ToArray();
        }

        /// <summary>
        /// Gets a perpendicular offset from the lane center, pointing outward (toward map edge).
        /// CORRECT LAYOUT: Lane at TOP with slight dip, outward = positive Z direction.
        /// </summary>
        private Vector3 GetLanePerpendicularOffset(float t, float distance)
        {
            // Get two nearby points to calculate lane direction
            float delta = 0.01f;
            float t1 = Mathf.Clamp01(t - delta);
            float t2 = Mathf.Clamp01(t + delta);

            Vector3 p1 = GetLanePointFallback(t1);
            Vector3 p2 = GetLanePointFallback(t2);

            // Lane direction (tangent)
            Vector3 tangent = (p2 - p1).normalized;

            // Perpendicular in XZ plane (rotate 90 degrees)
            Vector3 perpendicular = new Vector3(-tangent.z, 0, tangent.x);

            // Make sure it points outward (positive Z direction = toward map edge)
            if (perpendicular.z < 0)
            {
                perpendicular = -perpendicular;
            }

            return perpendicular * distance;
        }

        /// <summary>
        /// Fallback lane calculation if MapGenerator isn't available.
        /// Lane is STRAIGHT - no dips, no triangles.
        /// </summary>
        private Vector3 GetLanePointFallback(float t)
        {
            const float LANE_START_X = 20f;
            const float LANE_END_X = 80f;
            const float LANE_Z = 40f;

            float x = Mathf.Lerp(LANE_START_X, LANE_END_X, t);
            return new Vector3(x, 0.05f, LANE_Z);  // STRAIGHT - constant Z
        }

        private void Update()
        {
            // Spawn minions regardless of game state (game should always be playing)
            if (Time.time >= nextWaveTime)
            {
                Debug.Log($"MinionSpawner: Spawning wave {waveCount + 1}");
                SpawnWave();
                nextWaveTime = Time.time + waveInterval;
                waveCount++;
            }
        }

        private void SpawnWave()
        {
            StartCoroutine(SpawnWaveCoroutine(GameManager.Team.Blue));
            StartCoroutine(SpawnWaveCoroutine(GameManager.Team.Red));
        }

        private System.Collections.IEnumerator SpawnWaveCoroutine(GameManager.Team team)
        {
            Transform spawnPoint = team == GameManager.Team.Blue ? blueSpawnPoint : redSpawnPoint;
            Transform[] waypoints = team == GameManager.Team.Blue ? blueWaypoints : redWaypoints;

            if (spawnPoint == null)
            {
                spawnPoint = transform;
            }

            Vector3[] waypointPositions = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypointPositions[i] = waypoints[i].position;
            }

            // All minions spawn at the crystal, one by one
            // Delay is long enough for each to move out before next spawns
            Vector3 spawnPos = spawnPoint.position;

            // Spawn melee minions first (they lead the wave)
            for (int i = 0; i < meleePerWave; i++)
            {
                SpawnMinion(Minion.MinionType.Melee, team, spawnPos, waypointPositions);
                yield return new WaitForSeconds(spawnDelay);
            }

            // Spawn ranged minions behind melee
            for (int i = 0; i < rangedPerWave; i++)
            {
                SpawnMinion(Minion.MinionType.Ranged, team, spawnPos, waypointPositions);
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private void SpawnMinion(Minion.MinionType type, GameManager.Team team, Vector3 position, Vector3[] waypoints)
        {
            GameObject minionObj;

            GameObject prefab = type == Minion.MinionType.Melee ? meleeMinionPrefab : rangedMinionPrefab;

            if (prefab != null)
            {
                minionObj = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // Create placeholder minion
                minionObj = CreatePlaceholderMinion(type, team);
                minionObj.transform.position = position;
            }

            // Initialize minion
            var minion = minionObj.GetComponent<Minion>();
            if (minion == null)
            {
                minion = minionObj.AddComponent<Minion>();
            }

            minion.Initialize(team, waypoints);
        }

        private GameObject CreatePlaceholderMinion(Minion.MinionType type, GameManager.Team team)
        {
            GameObject minion = new GameObject($"{team}_{type}_Minion");

            // Visual
            GameObject visual;
            if (type == Minion.MinionType.Melee)
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.localScale = new Vector3(0.5f, 0.75f, 0.5f);
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
            }

            visual.transform.SetParent(minion.transform);
            visual.transform.localPosition = new Vector3(0, 0.5f, 0);

            // Color by team
            var renderer = visual.GetComponent<Renderer>();
            renderer.material.color = team == GameManager.Team.Blue ?
                new Color(0.3f, 0.5f, 0.9f) : new Color(0.9f, 0.3f, 0.3f);

            // Add required components
            var agent = minion.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.radius = 0.3f;
            agent.height = 1.5f;

            var stats = minion.AddComponent<Characters.CharacterStats>();
            var targetable = minion.AddComponent<Combat.Targetable>();
            targetable.SetTeam(team); // Set team immediately

            // Add collider for targeting
            var collider = minion.AddComponent<CapsuleCollider>();
            collider.radius = 0.3f;
            collider.height = 1.5f;
            collider.center = new Vector3(0, 0.75f, 0);

            return minion;
        }

        public int WaveCount => waveCount;
    }
}
