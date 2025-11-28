using UnityEngine;
using UnityEngine.Events;
using VaingloryMoba.UI;
using VaingloryMoba.Debugging;

namespace VaingloryMoba.Core
{
    /// <summary>
    /// Central game manager handling game state, initialization, and core systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject playerHeroPrefab;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;

        [Header("Game Settings")]
        [SerializeField] private float gameTime = 0f;
        [SerializeField] private bool isPaused = false;
        [SerializeField] private bool waitForHeroSelect = true;

        // Events
        public UnityEvent OnGameStart = new UnityEvent();
        public UnityEvent OnGamePause = new UnityEvent();
        public UnityEvent OnGameResume = new UnityEvent();
        public UnityEvent<Team> OnGameEnd = new UnityEvent<Team>();

        // State
        private GameObject playerHero;
        private GameState currentState = GameState.Initializing;
        private HeroSelectUI.HeroType selectedHeroType = HeroSelectUI.HeroType.Ringo;

        public enum GameState
        {
            Initializing,
            Playing,
            Paused,
            Ended
        }

        public enum Team
        {
            Blue,
            Red,
            Neutral
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

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                gameTime += Time.deltaTime;
            }
        }

        private void InitializeGame()
        {
            currentState = GameState.Initializing;

            // Create SceneDumper for debugging (press F12 to dump scene state)
            if (FindObjectOfType<SceneDumper>() == null)
            {
                var dumperObj = new GameObject("SceneDumper");
                dumperObj.AddComponent<SceneDumper>();
            }

            // If waiting for hero select, don't spawn yet
            if (waitForHeroSelect)
            {
                Debug.Log("Waiting for hero selection...");
                return;
            }

            // Spawn player hero with default (Ringo)
            SpawnPlayerHero(selectedHeroType);
            StartGame();
        }

        /// <summary>
        /// Called by HeroSelectUI when player picks a hero and clicks Start
        /// </summary>
        public void StartGameWithHero(HeroSelectUI.HeroType heroType)
        {
            selectedHeroType = heroType;
            SpawnPlayerHero(heroType);
            StartGame();
        }

        private void SpawnPlayerHero(HeroSelectUI.HeroType heroType)
        {
            // The hero is created by GameSceneSetup, we just need to signal which one
            Debug.Log($"Player selected hero: {heroType}");

            // Find existing player hero (already created by GameSceneSetup)
            var heroes = FindObjectsOfType<Characters.HeroController>();
            foreach (var hero in heroes)
            {
                if (hero.IsPlayerControlled)
                {
                    playerHero = hero.gameObject;
                    break;
                }
            }

            if (playerHero != null && GameCamera.Instance != null)
            {
                GameCamera.Instance.SetTarget(playerHero.transform);
            }
        }

        public void StartGame()
        {
            currentState = GameState.Playing;
            OnGameStart.Invoke();
            Debug.Log("Game Started");
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing)
                return;

            currentState = GameState.Paused;
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePause.Invoke();
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused)
                return;

            currentState = GameState.Playing;
            isPaused = false;
            Time.timeScale = 1f;
            OnGameResume.Invoke();
        }

        public void EndGame(Team winner)
        {
            currentState = GameState.Ended;
            OnGameEnd.Invoke(winner);
            Debug.Log($"=== GAME OVER === Winner: {winner} Team!");

            // Pause the game
            Time.timeScale = 0.1f; // Slow-mo for dramatic effect

            // Show victory message (simple UI)
            StartCoroutine(ShowVictoryMessage(winner));
        }

        private System.Collections.IEnumerator ShowVictoryMessage(Team winner)
        {
            yield return new WaitForSecondsRealtime(2f);

            // Create simple victory UI
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var victoryObj = new GameObject("VictoryMessage");
                victoryObj.transform.SetParent(canvas.transform);

                var rect = victoryObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(600, 200);

                var text = victoryObj.AddComponent<UnityEngine.UI.Text>();
                text.text = winner == Team.Blue ? "VICTORY!" : "DEFEAT!";
                text.fontSize = 72;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = winner == Team.Blue ? Color.cyan : Color.red;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            yield return new WaitForSecondsRealtime(3f);
            Time.timeScale = 1f;
        }

        public GameObject PlayerHero => playerHero;
        public GameState CurrentState => currentState;
        public float GameTime => gameTime;
        public bool IsPaused => isPaused;
    }
}
