using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;

namespace VaingloryMoba.Map
{
    /// <summary>
    /// Generates the Halcyon Fold map layout matching Vainglory's design (3v3).
    /// Map Scale: 160m x 40m
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map Dimensions")]
        [SerializeField] private float mapWidth = 160f;   // X-axis (horizontal)
        [SerializeField] private float mapHeight = 40f;   // Z-axis (vertical)
        [SerializeField] private float laneWidth = 8f;

        // ===========================================
        // FIXED COORDINATES (160x40 Grid)
        // ===========================================
        
        private const float LANE_Z = 35f;           // Lane Top Strip
        private const float KRAKEN_Z = 20f;         // Map Center Z (Vertical Center)
        private const float KRAKEN_X = 80f;         // Map Center X

        private const float BASE_WIDTH_X = 25f;     
        private const float BASE_HEIGHT_Z = 36f;    
        private const float BASE_CENTER_Z = 20f;    // Vertical Center
        
        private const float BLUE_BASE_X = 12f;      
        private const float RED_BASE_X = 148f;       

        private const float LANE_START_X = 25f;     
        private const float LANE_END_X = 135f;       

        [Header("Prefabs")]
        [SerializeField] private GameObject turretPrefab;
        [SerializeField] private GameObject crystalPrefab;
        
        [Header("Bush Prefabs")]
        [SerializeField] private GameObject bushStandard; // Round/Clumpy (Tri-bush, Lane)
        [SerializeField] private GameObject bushLong;     // Shop Mustache
        [SerializeField] private GameObject bushCorner;   // Mine/Large areas

        [Header("Materials")]
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material laneMaterial;
        [SerializeField] private Material jungleMaterial;

        // Generated references
        private GameObject mapContainer;
        private Turret[] blueTurrets;
        private Turret[] redTurrets;
        private VainCrystal blueCrystal;
        private VainCrystal redCrystal;

        public Transform BlueSpawnPoint { get; private set; }
        public Transform RedSpawnPoint { get; private set; }

        public VainCrystal BlueCrystal => blueCrystal;
        public VainCrystal RedCrystal => redCrystal;

        private void Awake()
        {
            LoadBushPrefabs();
            GenerateMap();
        }

        private void LoadBushPrefabs()
        {
            Debug.Log("[MapGenerator] LoadBushPrefabs called, bushStandard is null: " + (bushStandard == null));

            // Load bush prefab from Resources if not assigned in inspector
            if (bushStandard == null)
            {
                bushStandard = Resources.Load<GameObject>("Prefabs/Bush_Standard_Colored");
                if (bushStandard != null)
                {
                    Debug.Log("[MapGenerator] SUCCESS: Loaded bush prefab from Resources");
                }
                else
                {
                    Debug.LogWarning("[MapGenerator] FAILED: Could not load bush prefab from Resources/Prefabs/Bush_Standard_Colored");
                }
            }

            // Use same prefab for all bush types if specific ones aren't assigned
            if (bushLong == null) bushLong = bushStandard;
            if (bushCorner == null) bushCorner = bushStandard;

            Debug.Log("[MapGenerator] Bush prefabs - Standard: " + (bushStandard != null) +
                      ", Long: " + (bushLong != null) +
                      ", Corner: " + (bushCorner != null));
        }

        public void GenerateMap()
        {
            if (mapContainer != null)
            {
                DestroyImmediate(mapContainer);
            }

            mapContainer = new GameObject("Map");

            CreateGround();
            CreateBases();
            CreateLane();
            CreateJungle();
            CreateTurrets();
            CreateSpawnPoints();
            CreateNavMesh();
        }

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(mapContainer.transform);
            ground.transform.position = new Vector3(mapWidth / 2f, 0, mapHeight / 2f);
            ground.transform.localScale = new Vector3(mapWidth / 10f, 1, mapHeight / 10f);

            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            ground.layer = groundLayerIndex >= 0 ? groundLayerIndex : 0;

            var renderer = ground.GetComponent<Renderer>();
            if (groundMaterial != null) renderer.material = groundMaterial;
            else renderer.material.color = new Color(0.15f, 0.28f, 0.15f); 

            ground.GetComponent<Collider>().enabled = true;

            // DEBUG Corners
            CreateDebugMarker(mapContainer.transform, new Vector3(0, 0.5f, 0), Color.white, "Corner_0_0");
            CreateDebugMarker(mapContainer.transform, new Vector3(160, 0.5f, 0), Color.white, "Corner_160_0");
            CreateDebugMarker(mapContainer.transform, new Vector3(0, 0.5f, 40), Color.white, "Corner_0_40");
            CreateDebugMarker(mapContainer.transform, new Vector3(160, 0.5f, 40), Color.white, "Corner_160_40");
        }

        private void CreateDebugMarker(Transform parent, Vector3 pos, Color color, string name)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent);
            marker.transform.position = pos;
            marker.transform.localScale = new Vector3(1f, 5f, 1f);
            marker.GetComponent<Renderer>().material.color = color;
            Destroy(marker.GetComponent<Collider>());
        }

        private void CreateLane()
        {
            var laneContainer = new GameObject("Lane");
            laneContainer.transform.SetParent(mapContainer.transform);

            Color laneColor = new Color(0.45f, 0.42f, 0.35f);

            CreateLaneSegment(laneContainer.transform,
                new Vector3(LANE_START_X, 0.05f, LANE_Z),
                new Vector3(LANE_END_X, 0.05f, LANE_Z),
                laneColor, "Lane_Straight");

            // Kraken bowl (Visual indent)
            var krakenBowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            krakenBowl.name = "KrakenBowl";
            krakenBowl.transform.SetParent(laneContainer.transform);
            krakenBowl.transform.position = new Vector3(KRAKEN_X, 0.03f, LANE_Z - 6f);
            krakenBowl.transform.localScale = new Vector3(14f, 0.1f, 10f);
            krakenBowl.GetComponent<Renderer>().material.color = laneColor;
            Destroy(krakenBowl.GetComponent<Collider>());
        }

        private Vector3 GetLanePoint(float t)
        {
            float x = Mathf.Lerp(LANE_START_X, LANE_END_X, t);
            return new Vector3(x, 0.05f, LANE_Z);
        }

        private void CreateLaneSegment(Transform parent, Vector3 p1, Vector3 p2, Color color, string name)
        {
            Vector3 midpoint = (p1 + p2) / 2f;
            float length = Vector3.Distance(p1, p2);
            float angle = Mathf.Atan2(p2.z - p1.z, p2.x - p1.x) * Mathf.Rad2Deg;

            var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = name;
            segment.transform.SetParent(parent);
            segment.transform.position = midpoint;
            segment.transform.localScale = new Vector3(length + 0.5f, 0.1f, laneWidth);
            segment.transform.rotation = Quaternion.Euler(0, -angle, 0);
            segment.GetComponent<Renderer>().material.color = color;
            Destroy(segment.GetComponent<Collider>());
        }

        private void CreateJungle()
        {
            var jungleContainer = new GameObject("Jungle");
            jungleContainer.transform.SetParent(mapContainer.transform);

            // 1. KRAKEN (Center)
            CreateKrakenPit(jungleContainer.transform, new Vector3(KRAKEN_X, 0f, KRAKEN_Z));

            // 2. MINION MINES
            CreateMinionMine(jungleContainer.transform, new Vector3(45f, 0f, 22f), true); // Blue
            CreateMinionMine(jungleContainer.transform, new Vector3(115f, 0f, 22f), false); // Red

            // 3. SHOP (Bottom Center)
            CreateJungleShop(jungleContainer.transform, new Vector3(80f, 0f, 3f));

            // 3.5 CAMPS
            // Back Camps (Healing)
            CreateJungleCamp(jungleContainer.transform, new Vector3(25f, 0f, 10f), "Camp_Blue_Back1"); 
            CreateJungleCamp(jungleContainer.transform, new Vector3(35f, 0f, 10f), "Camp_Blue_Back2"); 
            
            CreateJungleCamp(jungleContainer.transform, new Vector3(135f, 0f, 10f), "Camp_Red_Back1"); 
            CreateJungleCamp(jungleContainer.transform, new Vector3(125f, 0f, 10f), "Camp_Red_Back2"); 

            // Front Camps (Objective)
            CreateJungleCamp(jungleContainer.transform, new Vector3(60f, 0f, 22f), "Camp_Blue_Front");
            CreateJungleCamp(jungleContainer.transform, new Vector3(100f, 0f, 22f), "Camp_Red_Front");

            // 4. WALLS (Only major dividers)
            float wallHeight = 2f;
            Color wallColor = new Color(0.15f, 0.25f, 0.15f);

            // Lane Dividers (Top) - Z=32
            CreateJungleWall(jungleContainer.transform, new Vector3(30f, wallHeight/2, 32f), new Vector3(15f, wallHeight, 2f), wallColor, "Wall_LaneDiv_Blue_1");
            CreateJungleWall(jungleContainer.transform, new Vector3(55f, wallHeight/2, 32f), new Vector3(10f, wallHeight, 2f), wallColor, "Wall_LaneDiv_Blue_2");
            CreateJungleWall(jungleContainer.transform, new Vector3(130f, wallHeight/2, 32f), new Vector3(15f, wallHeight, 2f), wallColor, "Wall_LaneDiv_Red_1");
            CreateJungleWall(jungleContainer.transform, new Vector3(105f, wallHeight/2, 32f), new Vector3(10f, wallHeight, 2f), wallColor, "Wall_LaneDiv_Red_2");

            // Kraken Walls (Center)
            CreateJungleWall(jungleContainer.transform, new Vector3(70f, 1f, 20f), new Vector3(2f, 2f, 8f), wallColor, "Wall_Kraken_L");
            CreateJungleWall(jungleContainer.transform, new Vector3(90f, 1f, 20f), new Vector3(2f, 2f, 8f), wallColor, "Wall_Kraken_R");

            // Center Block (Shop cover)
            CreateJungleWall(jungleContainer.transform, new Vector3(80f, 1f, 12f), new Vector3(16f, 2f, 2f), wallColor, "Wall_Center");

            // 4.5 ROCKS (Organic Obstacles)
            // These replace the internal walls to create "soft" corridors
            Color rockColor = new Color(0.3f, 0.3f, 0.35f);
            
            // Blue Side Rocks (Separating Back from Front)
            CreateRock(jungleContainer.transform, new Vector3(35f, 0, 16f), new Vector3(4f, 3f, 4f), rockColor);
            CreateRock(jungleContainer.transform, new Vector3(25f, 0, 18f), new Vector3(3f, 2f, 3f), rockColor);

            // Red Side Rocks
            CreateRock(jungleContainer.transform, new Vector3(125f, 0, 16f), new Vector3(4f, 3f, 4f), rockColor);
            CreateRock(jungleContainer.transform, new Vector3(135f, 0, 18f), new Vector3(3f, 2f, 3f), rockColor);

            // 5. BUSHES
            // Lane Ganks (Standard)
            CreateBush(jungleContainer.transform, new Vector3(40f, 0, 32f), new Vector2(3f, 2f), "Standard");
            CreateBush(jungleContainer.transform, new Vector3(120f, 0, 32f), new Vector2(3f, 2f), "Standard");
            
            // Shop Mustache (Long)
            CreateBush(jungleContainer.transform, new Vector3(72f, 0, 4f), new Vector2(6f, 3f), "Long");
            CreateBush(jungleContainer.transform, new Vector3(88f, 0, 4f), new Vector2(6f, 3f), "Long");

            // Tri-Bushes (Standard)
            CreateBush(jungleContainer.transform, new Vector3(75f, 0, 25f), new Vector2(2.5f, 2.5f), "Standard");
            CreateBush(jungleContainer.transform, new Vector3(85f, 0, 25f), new Vector2(2.5f, 2.5f), "Standard");

            // Mine Bushes (Corner)
            CreateBush(jungleContainer.transform, new Vector3(48f, 0, 18f), new Vector2(3f, 3f), "Corner"); // Blue Mine Cover
            CreateBush(jungleContainer.transform, new Vector3(112f, 0, 18f), new Vector2(3f, 3f), "Corner"); // Red Mine Cover
        }

        private void CreateRock(Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Sphere looks more organic than Cube
            rock.name = "Rock";
            rock.transform.SetParent(parent);
            rock.transform.position = new Vector3(position.x, scale.y * 0.3f, position.z); // Sunk into ground slightly
            rock.transform.localScale = scale;
            rock.GetComponent<Renderer>().material.color = color;
            // Keep collider
        }

        private void CreateKrakenPit(Transform parent, Vector3 position)
        {
            var pit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pit.name = "KrakenPit";
            pit.transform.SetParent(parent);
            pit.transform.position = new Vector3(position.x, -0.2f, position.z);
            pit.transform.localScale = new Vector3(14f, 0.5f, 14f);
            pit.GetComponent<Renderer>().material.color = new Color(0.2f, 0.35f, 0.45f); 
            Destroy(pit.GetComponent<Collider>());
        }

        private void CreateMinionMine(Transform parent, Vector3 position, bool isBlueSide)
        {
            var mine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mine.name = isBlueSide ? "MinionMine_Blue" : "MinionMine_Red";
            mine.transform.SetParent(parent);
            mine.transform.position = new Vector3(position.x, 0.05f, position.z);
            mine.transform.localScale = new Vector3(10f, 0.1f, 10f);
            mine.GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.6f);
            Destroy(mine.GetComponent<Collider>());
        }

        private void CreateJungleShop(Transform parent, Vector3 position)
        {
            var shop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shop.name = "JungleShop";
            shop.transform.SetParent(parent);
            shop.transform.position = new Vector3(position.x, 0.05f, position.z);
            shop.transform.localScale = new Vector3(12f, 0.1f, 6f);
            shop.GetComponent<Renderer>().material.color = new Color(0.8f, 0.7f, 0.4f);
            Destroy(shop.GetComponent<Collider>());
        }

        private void CreateJungleWall(Transform parent, Vector3 position, Vector3 size, Color color, string name)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = size;
            wall.GetComponent<Renderer>().material.color = color;
        }

        private void CreateBush(Transform parent, Vector3 position, Vector2 size, string type)
        {
            GameObject selectedPrefab = null;
            switch (type)
            {
                case "Long": selectedPrefab = bushLong; break;
                case "Corner": selectedPrefab = bushCorner; break;
                default: selectedPrefab = bushStandard; break;
            }

            // Fallback to standard if specific type is missing
            if (selectedPrefab == null) selectedPrefab = bushStandard;

            if (selectedPrefab != null)
            {
                // Place the prefab
                var bush = Instantiate(selectedPrefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
                bush.name = $"Bush_{type}";
                Debug.Log($"[MapGenerator] Created bush '{type}' at {position}");

                // Scale logic:
                bush.transform.localScale = new Vector3(size.x, 1.5f, size.y);
            }
            else
            {
                // Fallback: Green Cylinder
                var bush = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bush.name = "Bush_Placeholder";
                bush.transform.SetParent(parent);
                bush.transform.position = new Vector3(position.x, 0.15f, position.z);
                bush.transform.localScale = new Vector3(size.x, 0.3f, size.y);
                var renderer = bush.GetComponent<Renderer>();
                renderer.material.color = new Color(0.1f, 0.4f, 0.15f, 0.85f);
                Destroy(bush.GetComponent<Collider>());
            }
        }

        private void CreateJungleCamp(Transform parent, Vector3 position, string name)
        {
            var camp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            camp.name = name;
            camp.transform.SetParent(parent);
            camp.transform.position = new Vector3(position.x, 0.02f, position.z);
            camp.transform.localScale = new Vector3(5f, 0.1f, 5f);
            camp.GetComponent<Renderer>().material.color = new Color(0.3f, 0.25f, 0.2f);
            Destroy(camp.GetComponent<Collider>());
        }

        private void CreateBases()
        {
            CreateBase(new Vector3(BLUE_BASE_X, 0, BASE_CENTER_Z), GameManager.Team.Blue);
            CreateBase(new Vector3(RED_BASE_X, 0, BASE_CENTER_Z), GameManager.Team.Red);
        }

        private void CreateBase(Vector3 position, GameManager.Team team)
        {
            string teamName = team == GameManager.Team.Blue ? "Blue" : "Red";
            Color teamColor = team == GameManager.Team.Blue
                ? new Color(0.3f, 0.5f, 0.9f)
                : new Color(0.9f, 0.3f, 0.3f);
            Color teamColorLight = team == GameManager.Team.Blue
                ? new Color(0.4f, 0.6f, 1f, 0.6f)
                : new Color(1f, 0.4f, 0.4f, 0.6f);

            // Base Platform
            var basePlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlatform.name = $"{teamName}Base";
            basePlatform.transform.SetParent(mapContainer.transform);
            basePlatform.transform.position = position;
            basePlatform.transform.localScale = new Vector3(BASE_WIDTH_X, 0.3f, BASE_HEIGHT_Z);
            basePlatform.GetComponent<Renderer>().material.color = new Color(teamColor.r * 0.5f, teamColor.g * 0.5f, teamColor.b * 0.5f);
            Destroy(basePlatform.GetComponent<Collider>());

            // Fountain
            float spawnOffsetX = team == GameManager.Team.Blue ? -BASE_WIDTH_X * 0.35f : BASE_WIDTH_X * 0.35f;
            Vector3 spawnPos = new Vector3(position.x + spawnOffsetX, 0.15f, position.z);
            
            var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountain.name = $"{teamName}Fountain";
            fountain.transform.SetParent(mapContainer.transform);
            fountain.transform.position = spawnPos;
            fountain.transform.localScale = new Vector3(8f, 0.2f, 8f); 
            fountain.GetComponent<Renderer>().material.color = teamColorLight;
            Destroy(fountain.GetComponent<Collider>());

            // Spawn Wall
            var wallContainer = new GameObject($"{teamName}SpawnWall");
            wallContainer.transform.SetParent(mapContainer.transform);
            wallContainer.transform.position = spawnPos;
            
            float wallRadius = 4.5f;
            float baseAngle = team == GameManager.Team.Blue ? 180f : 0f;
            
            for (int i = -2; i <= 2; i++)
            {
                float angle = baseAngle + (i * 20f);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * wallRadius;
                
                var wallSeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallSeg.transform.SetParent(wallContainer.transform);
                wallSeg.transform.position = spawnPos + offset + Vector3.up * 1f;
                wallSeg.transform.localScale = new Vector3(1f, 2f, 3f);
                wallSeg.transform.LookAt(spawnPos);
                wallSeg.GetComponent<Renderer>().material.color = teamColor;
                Destroy(wallSeg.GetComponent<Collider>());
            }
        }

        private void CreateTurrets()
        {
            var turretsContainer = new GameObject("Turrets");
            turretsContainer.transform.SetParent(mapContainer.transform);

            blueTurrets = new Turret[5];
            redTurrets = new Turret[5];

            // BLUE
            blueTurrets[0] = CreateTurret(turretsContainer.transform, GetLanePoint(0.15f) + Vector3.forward * 2.5f, GameManager.Team.Blue, false, "Blue_Inner");
            blueTurrets[1] = CreateTurret(turretsContainer.transform, GetLanePoint(0.30f) + Vector3.forward * 2.5f, GameManager.Team.Blue, true, "Blue_Outer");
            blueTurrets[4] = CreateTurret(turretsContainer.transform, GetLanePoint(0.0f) + Vector3.forward * 2.5f, GameManager.Team.Blue, false, "Blue_Base");
            
            // Vain Turrets
            blueTurrets[2] = CreateTurret(turretsContainer.transform, new Vector3(BLUE_BASE_X + 6f, 0, 24f), GameManager.Team.Blue, false, "Blue_Vain_Top");
            blueTurrets[3] = CreateTurret(turretsContainer.transform, new Vector3(BLUE_BASE_X + 6f, 0, 16f), GameManager.Team.Blue, false, "Blue_Vain_Bottom");

            blueTurrets[0].outerTurret = blueTurrets[1];
            blueTurrets[4].outerTurret = blueTurrets[0];
            blueTurrets[2].outerTurret = blueTurrets[4];
            blueTurrets[3].outerTurret = blueTurrets[4];

            // RED
            redTurrets[0] = CreateTurret(turretsContainer.transform, GetLanePoint(0.85f) + Vector3.forward * 2.5f, GameManager.Team.Red, false, "Red_Inner");
            redTurrets[1] = CreateTurret(turretsContainer.transform, GetLanePoint(0.70f) + Vector3.forward * 2.5f, GameManager.Team.Red, true, "Red_Outer");
            redTurrets[4] = CreateTurret(turretsContainer.transform, GetLanePoint(1.0f) + Vector3.forward * 2.5f, GameManager.Team.Red, false, "Red_Base");

            redTurrets[2] = CreateTurret(turretsContainer.transform, new Vector3(RED_BASE_X - 6f, 0, 24f), GameManager.Team.Red, false, "Red_Vain_Top");
            redTurrets[3] = CreateTurret(turretsContainer.transform, new Vector3(RED_BASE_X - 6f, 0, 16f), GameManager.Team.Red, false, "Red_Vain_Bottom");

            redTurrets[0].outerTurret = redTurrets[1];
            redTurrets[4].outerTurret = redTurrets[0];
            redTurrets[2].outerTurret = redTurrets[4];
            redTurrets[3].outerTurret = redTurrets[4];

            CreateCrystals(turretsContainer.transform);
        }

        private Turret CreateTurret(Transform parent, Vector3 position, GameManager.Team team, bool isOuter, string name)
        {
            GameObject turretObj;
            if (turretPrefab != null) turretObj = Instantiate(turretPrefab, position, Quaternion.identity, parent);
            else
            {
                turretObj = new GameObject(name);
                turretObj.transform.SetParent(parent);
                turretObj.transform.position = position;
                var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseObj.transform.SetParent(turretObj.transform);
                baseObj.transform.localPosition = new Vector3(0, 1f, 0);
                baseObj.transform.localScale = new Vector3(2f, 2f, 2f);
                baseObj.GetComponent<Renderer>().material.color = team == GameManager.Team.Blue ? new Color(0.3f, 0.4f, 0.8f) : new Color(0.8f, 0.3f, 0.3f);
                Destroy(baseObj.GetComponent<Collider>());
            }
            
            turretObj.name = name;
            var collider = turretObj.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 2f, 0);
            collider.radius = 1.5f;
            collider.height = 4f;
            var targetable = turretObj.AddComponent<Targetable>();
            targetable.SetTeam(team);
            var stats = turretObj.AddComponent<CharacterStats>();
            stats.SetBaseStats(500f, 160f, 0f);
            var turret = turretObj.AddComponent<Turret>();
            turret.SetTeam(team);
            turret.SetIsOuterTurret(isOuter);
            var healthBar = new GameObject("HealthBar").AddComponent<UI.WorldHealthBar>();
            healthBar.Initialize(turretObj.transform, team == GameManager.Team.Red);
            return turret;
        }

        private void CreateCrystals(Transform parent)
        {
            blueCrystal = CreateCrystal(parent, new Vector3(BLUE_BASE_X, 0, 20f), GameManager.Team.Blue, "BlueCrystal", blueTurrets);
            redCrystal = CreateCrystal(parent, new Vector3(RED_BASE_X, 0, 20f), GameManager.Team.Red, "RedCrystal", redTurrets);
        }

        private VainCrystal CreateCrystal(Transform parent, Vector3 position, GameManager.Team team, string name, Turret[] turrets)
        {
            GameObject crystalObj;
            if (crystalPrefab != null) crystalObj = Instantiate(crystalPrefab, position, Quaternion.identity, parent);
            else
            {
                crystalObj = new GameObject(name);
                crystalObj.transform.SetParent(parent);
                crystalObj.transform.position = position;
                var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crystal.transform.SetParent(crystalObj.transform);
                crystal.transform.localPosition = new Vector3(0, 2f, 0);
                crystal.transform.localScale = new Vector3(2f, 4f, 2f);
                crystal.GetComponent<Renderer>().material.color = team == GameManager.Team.Blue ? new Color(0.4f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f);
            }
            crystalObj.name = name;
            var collider = crystalObj.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 2f, 0);
            collider.size = new Vector3(3f, 4f, 3f);
            var targetable = crystalObj.AddComponent<Targetable>();
            targetable.SetTeam(team);
            var stats = crystalObj.AddComponent<CharacterStats>();
            stats.SetBaseStats(500f, 0f, 0f);
            var vainCrystal = crystalObj.AddComponent<VainCrystal>();
            vainCrystal.Initialize(team, turrets);
            var healthBar = new GameObject("HealthBar").AddComponent<UI.WorldHealthBar>();
            healthBar.Initialize(crystalObj.transform, team == GameManager.Team.Red);
            return vainCrystal;
        }

        private void CreateSpawnPoints()
        {
            var blueSpawn = new GameObject("BlueSpawnPoint");
            blueSpawn.transform.SetParent(mapContainer.transform);
            blueSpawn.transform.position = new Vector3(BLUE_BASE_X - 5f, 0.1f, BASE_CENTER_Z);
            BlueSpawnPoint = blueSpawn.transform;

            var redSpawn = new GameObject("RedSpawnPoint");
            redSpawn.transform.SetParent(mapContainer.transform);
            redSpawn.transform.position = new Vector3(RED_BASE_X + 5f, 0.1f, BASE_CENTER_Z);
            RedSpawnPoint = redSpawn.transform;
        }

        private void CreateNavMesh()
        {
            var navMeshSurface = mapContainer.AddComponent<NavMeshSurface>();
            if (navMeshSurface != null)
            {
                navMeshSurface.collectObjects = CollectObjects.All;
                navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                navMeshSurface.BuildNavMesh();
            }
        }

        public Vector3 GetLanePosition(float t)
        {
            return GetLanePoint(t);
        }

        public float GetBaseZ()
        {
            return BASE_CENTER_Z;
        }

        public float GetLaneZ()
        {
            return LANE_Z;
        }
    }
}