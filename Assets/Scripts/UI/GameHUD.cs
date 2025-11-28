using UnityEngine;
using UnityEngine.UI;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;

namespace VaingloryMoba.UI
{
    /// <summary>
    /// Main HUD controller for the game interface.
    /// Manages health/energy bars, ability buttons, and other UI elements.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        [Header("Player Stats")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Text energyText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text goldText;

        [Header("Ability Buttons")]
        [SerializeField] private AbilityButton[] abilityButtons = new AbilityButton[4];

        [Header("Targeting")]
        [SerializeField] private GameObject targetingIndicator;
        [SerializeField] private LineRenderer skillshotLine;
        [SerializeField] private GameObject rangeCircle;

        [Header("Move Indicator")]
        [SerializeField] private GameObject moveIndicatorPrefab;

        // References
        private HeroController playerHero;
        private CharacterStats playerStats;
        private GameObject moveIndicator;

        // Targeting state
        private bool isTargeting;
        private int targetingAbilityIndex;
        private AbilityBase targetingAbility;

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
            // Create move indicator
            if (moveIndicatorPrefab != null)
            {
                moveIndicator = Instantiate(moveIndicatorPrefab);
                moveIndicator.SetActive(false);
            }

            // Find player hero
            StartCoroutine(WaitForPlayer());
        }

        private System.Collections.IEnumerator WaitForPlayer()
        {
            while (GameManager.Instance == null || GameManager.Instance.PlayerHero == null)
            {
                yield return null;
            }

            SetupPlayerHero(GameManager.Instance.PlayerHero);
        }

        private void SetupPlayerHero(GameObject hero)
        {
            playerHero = hero.GetComponent<HeroController>();
            playerStats = hero.GetComponent<CharacterStats>();

            if (playerStats != null)
            {
                playerStats.OnHealthChanged.AddListener(UpdateHealthBar);
                playerStats.OnEnergyChanged.AddListener(UpdateEnergyBar);
                playerStats.OnGoldChanged.AddListener(UpdateGold);
                playerStats.OnLevelUp.AddListener(UpdateLevel);

                // Initial update
                UpdateHealthBar(playerStats.CurrentHealth, playerStats.MaxHealth);
                UpdateEnergyBar(playerStats.CurrentEnergy, playerStats.MaxEnergy);
                UpdateGold(playerStats.Gold);
                UpdateLevel(playerStats.Level);
            }

            // Setup ability buttons
            SetupAbilityButtons();
        }

        private void SetupAbilityButtons()
        {
            if (playerHero == null) return;

            for (int i = 0; i < abilityButtons.Length; i++)
            {
                var ability = playerHero.GetAbility(i);
                if (ability != null && abilityButtons[i] != null)
                {
                    abilityButtons[i].Setup(ability, i);
                }
            }
        }

        private void Update()
        {
            UpdateAbilityButtons();
            UpdateTargeting();
        }

        private void UpdateAbilityButtons()
        {
            if (playerStats == null) return;

            foreach (var button in abilityButtons)
            {
                if (button != null)
                {
                    button.UpdateState();
                }
            }
        }

        private void UpdateTargeting()
        {
            if (!isTargeting || targetingAbility == null) return;

            // Get touch/mouse position
            Vector2 inputPos = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                inputPos = Input.GetTouch(0).position;
            }

            // Convert to world position
            Vector3? worldPos = TouchInputManager.Instance?.ScreenToGroundPosition(inputPos);
            if (!worldPos.HasValue) return;

            // Update targeting indicator based on ability type
            switch (targetingAbility.Targeting)
            {
                case AbilityBase.TargetingType.Skillshot:
                    UpdateSkillshotIndicator(worldPos.Value);
                    if (rangeCircle != null) rangeCircle.SetActive(false); // Skillshots use line, not circle
                    break;

                case AbilityBase.TargetingType.PointTarget:
                    UpdatePointTargetIndicator(worldPos.Value);
                    break;

                case AbilityBase.TargetingType.UnitTarget:
                    // For unit target abilities, hide the range circle
                    // The player needs to tap on an enemy to target them
                    if (rangeCircle != null) rangeCircle.SetActive(false);
                    break;
            }
        }

        private void UpdateSkillshotIndicator(Vector3 targetPos)
        {
            if (playerHero == null || skillshotLine == null) return;

            Vector3 heroPos = playerHero.transform.position;
            Vector3 direction = (targetPos - heroPos).normalized;
            direction.y = 0;

            // Clamp to range
            float distance = Vector3.Distance(heroPos, targetPos);
            distance = Mathf.Min(distance, targetingAbility.Range);
            Vector3 endPos = heroPos + direction * distance;

            heroPos.y = 0.1f;
            endPos.y = 0.1f;

            skillshotLine.gameObject.SetActive(true);
            skillshotLine.SetPosition(0, heroPos);
            skillshotLine.SetPosition(1, endPos);
        }

        private void UpdatePointTargetIndicator(Vector3 targetPos)
        {
            if (rangeCircle == null) return;

            rangeCircle.SetActive(true);
            rangeCircle.transform.position = targetPos + Vector3.up * 0.1f;
            rangeCircle.transform.localScale = Vector3.one * targetingAbility.Radius * 2f;
        }

        #region Public Methods

        public void StartAbilityTargeting(int abilityIndex, AbilityBase ability)
        {
            isTargeting = true;
            targetingAbilityIndex = abilityIndex;
            targetingAbility = ability;

            TouchInputManager.Instance?.StartAbilityTargeting(abilityIndex);

            // Show range circle around hero
            if (rangeCircle != null && playerHero != null)
            {
                // Create range indicator
            }
        }

        public void CancelAbilityTargeting()
        {
            isTargeting = false;
            targetingAbilityIndex = -1;
            targetingAbility = null;

            TouchInputManager.Instance?.CancelAbilityTargeting();

            if (skillshotLine != null) skillshotLine.gameObject.SetActive(false);
            if (rangeCircle != null) rangeCircle.SetActive(false);
        }

        public void ConfirmAbilityTarget(Vector3 position)
        {
            if (!isTargeting || playerHero == null) return;

            playerHero.UseAbility(targetingAbilityIndex, position, null);
            CancelAbilityTargeting();
        }

        public void ConfirmAbilityTarget(GameObject target)
        {
            if (!isTargeting || playerHero == null) return;

            playerHero.UseAbility(targetingAbilityIndex, null, target);
            CancelAbilityTargeting();
        }

        public void ShowMoveIndicator(Vector3 position)
        {
            if (moveIndicator == null) return;

            moveIndicator.SetActive(true);
            moveIndicator.transform.position = position + Vector3.up * 0.1f;

            // Auto-hide after delay
            CancelInvoke(nameof(HideMoveIndicator));
            Invoke(nameof(HideMoveIndicator), 0.5f);
        }

        private void HideMoveIndicator()
        {
            if (moveIndicator != null)
            {
                moveIndicator.SetActive(false);
            }
        }

        #endregion

        #region UI Updates

        private void UpdateHealthBar(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }
        }

        private void UpdateEnergyBar(float current, float max)
        {
            if (energyBar != null)
            {
                energyBar.maxValue = max;
                energyBar.value = current;
            }

            if (energyText != null)
            {
                energyText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }
        }

        private void UpdateGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = gold.ToString();
            }
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }
        }

        #endregion

        public bool IsTargeting => isTargeting;
    }
}
