using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;
using VaingloryMoba.Map;
using VaingloryMoba.UI;

namespace VaingloryMoba.Core
{
    /// <summary>
    /// Sets up the entire game scene at runtime.
    /// Creates all necessary objects, managers, and initial state.
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("Hero Settings")]
        [SerializeField] private bool createPlayerHero = true;
        [SerializeField] private bool createEnemyAI = true;

        [Header("Map Settings")]
        [SerializeField] private bool generateMap = true;

        [Header("Systems")]
        [SerializeField] private bool enableMinions = true;
        [SerializeField] private bool enableJungle = true;

        private void Awake()
        {
            SetupLayers();
            SetupCamera();
            SetupManagers();

            if (generateMap)
            {
                SetupMap();
            }

            SetupUI();

            if (createPlayerHero)
            {
                CreatePlayerHero();
            }

            if (createEnemyAI)
            {
                CreateEnemyAI();
            }

            if (enableMinions)
            {
                SetupMinions();
            }

            if (enableJungle)
            {
                SetupJungle();
            }

            // Build NavMesh after map is created
            BuildNavMesh();
        }

        private void SetupLayers()
        {
            // Note: Layers need to be set up in Project Settings
            // This is a reminder of required layers:
            // - Ground (for raycast targeting)
            // - Unit (for unit selection)
            // - Obstacle (for pathfinding obstacles)
        }

        private void SetupCamera()
        {
            // Create main camera if none exists
            if (Camera.main == null)
            {
                var cameraObj = new GameObject("Main Camera");
                cameraObj.tag = "MainCamera";
                var camera = cameraObj.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.1f, 0.15f, 0.1f);
                camera.fieldOfView = 60f;
                cameraObj.AddComponent<AudioListener>();

                // Add game camera controller
                var gameCamera = cameraObj.AddComponent<GameCamera>();
            }
            else
            {
                // Add game camera to existing
                if (Camera.main.GetComponent<GameCamera>() == null)
                {
                    Camera.main.gameObject.AddComponent<GameCamera>();
                }
            }
        }

        private void SetupManagers()
        {
            // Create managers container
            var managers = new GameObject("Managers");

            // Game Manager
            if (FindObjectOfType<GameManager>() == null)
            {
                managers.AddComponent<GameManager>();
            }

            // Input Manager
            if (FindObjectOfType<TouchInputManager>() == null)
            {
                managers.AddComponent<TouchInputManager>();
            }

            // Respawn Manager
            if (FindObjectOfType<RespawnManager>() == null)
            {
                managers.AddComponent<RespawnManager>();
            }

            // Create EventSystem for UI
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void SetupMap()
        {
            if (FindObjectOfType<MapGenerator>() == null)
            {
                var mapObj = new GameObject("MapGenerator");
                mapObj.AddComponent<MapGenerator>();
            }
        }

        private void SetupUI()
        {
            // Create UI Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("UI Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Game HUD
            if (FindObjectOfType<GameHUD>() == null)
            {
                var hudObj = new GameObject("GameHUD");
                hudObj.transform.SetParent(canvas.transform);
                hudObj.AddComponent<GameHUD>();
                CreateHUDElements(hudObj.transform);
            }

            // Shop UI
            if (FindObjectOfType<ShopUI>() == null)
            {
                var shopObj = new GameObject("ShopUI");
                shopObj.transform.SetParent(canvas.transform);
                shopObj.AddComponent<ShopUI>();
            }

            // Hero Select UI (shown at game start)
            if (FindObjectOfType<HeroSelectUI>() == null)
            {
                var heroSelectObj = new GameObject("HeroSelectUI");
                heroSelectObj.transform.SetParent(canvas.transform);
                heroSelectObj.AddComponent<HeroSelectUI>();
            }

            // Ability Bar UI (bottom of screen)
            if (FindObjectOfType<AbilityBarUI>() == null)
            {
                var abilityBarObj = new GameObject("AbilityBarUI");
                abilityBarObj.transform.SetParent(canvas.transform);
                abilityBarObj.AddComponent<AbilityBarUI>();
            }
        }

        private void CreateHUDElements(Transform parent)
        {
            // Create ability buttons container
            var abilityBar = new GameObject("AbilityBar");
            abilityBar.transform.SetParent(parent);
            var abilityRect = abilityBar.AddComponent<RectTransform>();
            abilityRect.anchorMin = new Vector2(0.5f, 0f);
            abilityRect.anchorMax = new Vector2(0.5f, 0f);
            abilityRect.pivot = new Vector2(0.5f, 0f);
            abilityRect.anchoredPosition = new Vector2(0, 20);
            abilityRect.sizeDelta = new Vector2(400, 80);

            // Add horizontal layout
            var layout = abilityBar.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Create 4 ability buttons
            for (int i = 0; i < 4; i++)
            {
                var buttonObj = new GameObject($"AbilityButton_{i}");
                buttonObj.transform.SetParent(abilityBar.transform);

                var buttonRect = buttonObj.AddComponent<RectTransform>();
                buttonRect.sizeDelta = new Vector2(70, 70);

                var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
                image.color = new Color(0.2f, 0.2f, 0.3f);

                var button = buttonObj.AddComponent<AbilityButton>();

                // Cooldown overlay
                var overlay = new GameObject("CooldownOverlay");
                overlay.transform.SetParent(buttonObj.transform);
                var overlayRect = overlay.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.sizeDelta = Vector2.zero;
                var overlayImage = overlay.AddComponent<UnityEngine.UI.Image>();
                overlayImage.color = new Color(0, 0, 0, 0.7f);
                overlayImage.type = UnityEngine.UI.Image.Type.Filled;
                overlayImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
            }

            // Health/Energy bars
            CreateResourceBars(parent);

            // Gold display
            CreateGoldDisplay(parent);
        }

        private void CreateResourceBars(Transform parent)
        {
            var barsContainer = new GameObject("ResourceBars");
            barsContainer.transform.SetParent(parent);
            var barsRect = barsContainer.AddComponent<RectTransform>();
            barsRect.anchorMin = new Vector2(0, 1);
            barsRect.anchorMax = new Vector2(0, 1);
            barsRect.pivot = new Vector2(0, 1);
            barsRect.anchoredPosition = new Vector2(20, -20);
            barsRect.sizeDelta = new Vector2(200, 50);

            // Health bar
            CreateBar(barsContainer.transform, "HealthBar", new Color(0.2f, 0.8f, 0.2f), 0);

            // Energy bar
            CreateBar(barsContainer.transform, "EnergyBar", new Color(0.2f, 0.5f, 0.9f), -25);
        }

        private void CreateBar(Transform parent, string name, Color fillColor, float yOffset)
        {
            var bar = new GameObject(name);
            bar.transform.SetParent(parent);
            var rect = bar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, yOffset);
            rect.sizeDelta = new Vector2(0, 20);

            // Background
            var bg = bar.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f);

            // Fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(bar.transform);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImage = fill.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = fillColor;

            // Add slider component
            var slider = bar.AddComponent<UnityEngine.UI.Slider>();
            slider.fillRect = fillRect;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
        }

        private void CreateGoldDisplay(Transform parent)
        {
            var goldObj = new GameObject("GoldDisplay");
            goldObj.transform.SetParent(parent);
            var rect = goldObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -80);
            rect.sizeDelta = new Vector2(150, 30);

            var text = goldObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Gold: 500";
            text.color = new Color(1f, 0.85f, 0.2f);
            text.fontSize = 20;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void CreatePlayerHero()
        {
            var mapGen = FindObjectOfType<MapGenerator>();
            Vector3 spawnPos = mapGen != null ? mapGen.BlueSpawnPoint.position : new Vector3(40, 0, 5);

            var hero = CreateHero("PlayerHero", spawnPos, GameManager.Team.Blue, true);

            // Set camera to follow player hero
            if (GameCamera.Instance != null)
            {
                GameCamera.Instance.SetTarget(hero.transform);
            }

            // Set as player for input handling
            var heroController = hero.GetComponent<HeroController>();
            if (heroController != null)
            {
                heroController.SetAsPlayer(true);
            }
        }

        private void CreateEnemyAI()
        {
            var mapGen = FindObjectOfType<MapGenerator>();
            Vector3 spawnPos = mapGen != null ? mapGen.RedSpawnPoint.position : new Vector3(40, 0, 55);

            var hero = CreateHero("EnemyHero", spawnPos, GameManager.Team.Red, false);

            // Mark as AI-controlled (not player)
            var heroController = hero.GetComponent<HeroController>();
            if (heroController != null)
            {
                heroController.SetAsPlayer(false);
            }

            // Add AI controller
            hero.AddComponent<AIController>();
        }

        private GameObject CreateHero(string name, Vector3 position, GameManager.Team team, bool isPlayer)
        {
            var heroObj = new GameObject(name);
            heroObj.transform.position = position;

            // Visual placeholder
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.SetParent(heroObj.transform);
            visual.transform.localPosition = new Vector3(0, 1, 0);
            visual.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

            var renderer = visual.GetComponent<Renderer>();
            renderer.material.color = team == GameManager.Team.Blue ?
                new Color(0.2f, 0.4f, 0.9f) : new Color(0.9f, 0.3f, 0.3f);

            // Direction indicator (forward facing)
            var dirIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dirIndicator.transform.SetParent(heroObj.transform);
            dirIndicator.transform.localPosition = new Vector3(0, 1, 0.6f);
            dirIndicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            dirIndicator.GetComponent<Renderer>().material.color = Color.white;
            Destroy(dirIndicator.GetComponent<Collider>());

            // Components
            var agent = heroObj.AddComponent<NavMeshAgent>();
            agent.radius = 0.4f;
            agent.height = 2f;
            agent.speed = 3.3f;

            var stats = heroObj.AddComponent<CharacterStats>();
            stats.SetIsHero(true); // Enable passive gold income
            stats.AddGold(500); // Starting gold
            var motor = heroObj.AddComponent<CharacterMotor>();
            var heroController = heroObj.AddComponent<HeroController>();
            var targetable = heroObj.AddComponent<Targetable>();
            var inventory = heroObj.AddComponent<Inventory>();

            // Set team on components
            targetable.SetTeam(team);
            heroController.SetTeam(team);

            // Ringo-specific components
            var ringo = heroObj.AddComponent<RingoHero>();

            // Create abilities as children
            CreateAbilities(heroObj);

            // Collider for targeting
            var collider = heroObj.AddComponent<CapsuleCollider>();
            collider.radius = 0.4f;
            collider.height = 2f;
            collider.center = new Vector3(0, 1, 0);

            // Health bar
            var healthBar = new GameObject("HealthBar").AddComponent<WorldHealthBar>();
            healthBar.Initialize(heroObj.transform, team == GameManager.Team.Red);

            // Set layer for raycasting
            heroObj.layer = LayerMask.NameToLayer("Default"); // Should be "Unit" layer

            return heroObj;
        }

        private void CreateAbilities(GameObject hero)
        {
            var abilitiesContainer = new GameObject("Abilities");
            abilitiesContainer.transform.SetParent(hero.transform);

            // A - Achilles Shot
            var achillesShot = new GameObject("AchillesShot");
            achillesShot.transform.SetParent(abilitiesContainer.transform);
            achillesShot.AddComponent<AchillesShot>();

            // B - Twirling Silver
            var twirlingSilver = new GameObject("TwirlingSilver");
            twirlingSilver.transform.SetParent(abilitiesContainer.transform);
            twirlingSilver.AddComponent<TwirlingSilver>();

            // Ultimate - Hellfire Brew
            var hellfireBrew = new GameObject("HellfireBrew");
            hellfireBrew.transform.SetParent(abilitiesContainer.transform);
            hellfireBrew.AddComponent<HellfireBrew>();
        }

        private void SetupMinions()
        {
            if (FindObjectOfType<MinionSpawner>() == null)
            {
                var spawner = new GameObject("MinionSpawner");
                spawner.AddComponent<MinionSpawner>();
            }
        }

        private void SetupJungle()
        {
            if (FindObjectOfType<JungleManager>() == null)
            {
                var jungle = new GameObject("JungleManager");
                jungle.AddComponent<JungleManager>();
            }
        }

        private void BuildNavMesh()
        {
            // In Editor, NavMesh should be baked
            // At runtime, we'd need NavMeshSurface from AI Navigation package

            var surface = FindObjectOfType<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
            }
            else
            {
                Debug.LogWarning("NavMeshSurface not found. NavMesh needs to be baked in Editor or add NavMeshSurface component.");
            }
        }
    }
}
