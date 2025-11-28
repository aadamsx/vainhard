using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VaingloryMoba.Core;
using VaingloryMoba.Characters;
using VaingloryMoba.Combat;

namespace VaingloryMoba.UI
{
    /// <summary>
    /// Ability bar at bottom of screen with 3 abilities + ultimate.
    /// </summary>
    public class AbilityBarUI : MonoBehaviour
    {
        public static AbilityBarUI Instance { get; private set; }

        private GameObject abilityBar;
        private AbilitySlot[] slots = new AbilitySlot[3];
        private HeroController playerHero;
        private CharacterStats playerStats;

        private class AbilitySlot
        {
            public GameObject container;
            public Image background;
            public Image cooldownOverlay;
            public Text cooldownText;
            public Text keyText;
            public Text nameText;
            public Button button;
            public AbilityBase ability;
            public int index;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            CreateAbilityBar();
            // Hide until game starts
            if (abilityBar != null)
                abilityBar.SetActive(false);
            StartCoroutine(WaitForPlayer());
        }

        private void CreateAbilityBar()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Main container at bottom center
            abilityBar = new GameObject("AbilityBar");
            abilityBar.transform.SetParent(canvas.transform, false);
            var barRect = abilityBar.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0);
            barRect.anchorMax = new Vector2(0.5f, 0);
            barRect.pivot = new Vector2(0.5f, 0);
            barRect.anchoredPosition = new Vector2(0, 10);
            barRect.sizeDelta = new Vector2(320, 90);

            var barImage = abilityBar.AddComponent<Image>();
            barImage.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            // Layout for buttons
            var layout = abilityBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Create 3 ability slots (A, B, C/Ult)
            string[] keys = { "A", "B", "C" };
            Color[] colors = {
                new Color(0.3f, 0.5f, 0.8f),   // Blue for A
                new Color(0.5f, 0.7f, 0.3f),   // Green for B
                new Color(0.8f, 0.5f, 0.2f)    // Orange for Ult
            };

            for (int i = 0; i < 3; i++)
            {
                CreateAbilitySlot(i, keys[i], colors[i]);
            }

            Debug.Log("AbilityBarUI created");
        }

        private void CreateAbilitySlot(int index, string key, Color color)
        {
            var slot = new AbilitySlot { index = index };

            // Container
            slot.container = new GameObject($"Ability_{key}");
            slot.container.transform.SetParent(abilityBar.transform, false);
            var rect = slot.container.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90, 70);

            // Add LayoutElement to enforce size in HorizontalLayoutGroup
            var layoutElement = slot.container.AddComponent<LayoutElement>();
            layoutElement.minWidth = 90;
            layoutElement.minHeight = 70;
            layoutElement.preferredWidth = 90;
            layoutElement.preferredHeight = 70;

            // Background
            slot.background = slot.container.AddComponent<Image>();
            slot.background.color = color * 0.6f;

            // Button
            slot.button = slot.container.AddComponent<Button>();
            int idx = index;
            slot.button.onClick.AddListener(() => OnAbilityClicked(idx));

            // Add event trigger for drag
            var trigger = slot.container.AddComponent<EventTrigger>();
            AddDragEvents(trigger, index);

            // Cooldown overlay
            var overlayObj = new GameObject("CooldownOverlay");
            overlayObj.transform.SetParent(slot.container.transform, false);
            var overlayRect = overlayObj.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            slot.cooldownOverlay = overlayObj.AddComponent<Image>();
            slot.cooldownOverlay.color = new Color(0, 0, 0, 0.7f);
            slot.cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            slot.cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
            slot.cooldownOverlay.fillClockwise = false;
            slot.cooldownOverlay.type = Image.Type.Filled;
            slot.cooldownOverlay.fillAmount = 0;

            // Key label (Q, W, E or 1, 2, 3)
            var keyObj = CreateText(slot.container.transform, "Key", key, 18, Color.white,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(5, -5));
            slot.keyText = keyObj.GetComponent<Text>();
            slot.keyText.alignment = TextAnchor.UpperLeft;

            // Ability name
            var nameObj = CreateText(slot.container.transform, "Name", "-", 11, Color.white,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8));
            slot.nameText = nameObj.GetComponent<Text>();

            // Cooldown text (center)
            var cdObj = CreateText(slot.container.transform, "Cooldown", "", 20, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
            slot.cooldownText = cdObj.GetComponent<Text>();
            slot.cooldownText.gameObject.SetActive(false);

            slots[index] = slot;
        }

        private void AddDragEvents(EventTrigger trigger, int index)
        {
            // Pointer down
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((data) => OnPointerDown(index, (PointerEventData)data));
            trigger.triggers.Add(pointerDown);

            // Drag
            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener((data) => OnDrag(index, (PointerEventData)data));
            trigger.triggers.Add(drag);

            // Pointer up
            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((data) => OnPointerUp(index, (PointerEventData)data));
            trigger.triggers.Add(pointerUp);
        }

        private GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(80, 25);

            var text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;

            return obj;
        }

        private System.Collections.IEnumerator WaitForPlayer()
        {
            Debug.Log("AbilityBarUI: WaitForPlayer started");

            // Wait for GameManager
            while (GameManager.Instance == null)
                yield return null;

            // Subscribe to game start event as backup
            GameManager.Instance.OnGameStart.AddListener(OnGameStarted);

            // Wait for player hero
            float timeout = 30f;
            while (GameManager.Instance.PlayerHero == null && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (GameManager.Instance.PlayerHero != null)
            {
                Debug.Log("AbilityBarUI: PlayerHero found, setting up");
                // Show ability bar now that game has started
                if (abilityBar != null)
                    abilityBar.SetActive(true);

                SetupPlayer(GameManager.Instance.PlayerHero);
            }
            else
            {
                Debug.LogWarning("AbilityBarUI: Timeout waiting for PlayerHero");
            }
        }

        private void OnGameStarted()
        {
            Debug.Log("AbilityBarUI: OnGameStarted event received");
            if (GameManager.Instance.PlayerHero != null && playerHero == null)
            {
                if (abilityBar != null)
                    abilityBar.SetActive(true);
                SetupPlayer(GameManager.Instance.PlayerHero);
            }
        }

        private void SetupPlayer(GameObject hero)
        {
            playerHero = hero.GetComponent<HeroController>();
            playerStats = hero.GetComponent<CharacterStats>();

            Debug.Log($"AbilityBarUI: SetupPlayer called. Hero={hero.name}, HeroController={playerHero != null}");

            if (playerHero != null)
            {
                // Start coroutine to fetch abilities (may need to wait for HeroController.Start())
                StartCoroutine(FetchAbilitiesWhenReady());
            }
            else
            {
                Debug.LogWarning("AbilityBarUI: No HeroController found on player hero!");
            }
        }

        private System.Collections.IEnumerator FetchAbilitiesWhenReady()
        {
            // Wait a frame for HeroController.Start() to run and populate abilities
            yield return null;
            yield return null; // Extra frame for safety

            // Get abilities from hero
            for (int i = 0; i < 3; i++)
            {
                var ability = playerHero.GetAbility(i);
                Debug.Log($"AbilityBarUI: Slot {i} - ability={ability != null}, slot={slots[i] != null}");
                if (ability != null && slots[i] != null)
                {
                    slots[i].ability = ability;
                    slots[i].nameText.text = ability.AbilityName;
                    Debug.Log($"AbilityBarUI: Ability {i} set to: {ability.AbilityName}");
                }
                else if (slots[i] != null)
                {
                    // Show placeholder for missing ability
                    slots[i].nameText.text = $"Skill {i + 1}";
                }
            }
        }

        private void Update()
        {
            UpdateSlots();
            HandleKeyboardInput();
        }

        private void UpdateSlots()
        {
            if (playerStats == null) return;

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null || slot.ability == null) continue;

                bool isReady = slot.ability.IsReady;
                bool canAfford = slot.ability.CanAfford;

                // Update cooldown overlay
                slot.cooldownOverlay.fillAmount = slot.ability.CooldownPercent;

                // Update cooldown text
                if (!isReady)
                {
                    float cd = slot.ability.CurrentCooldown;
                    slot.cooldownText.text = cd >= 1f ? Mathf.CeilToInt(cd).ToString() : cd.ToString("F1");
                    slot.cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    slot.cooldownText.gameObject.SetActive(false);
                }

                // Update background color
                if (!isReady)
                {
                    slot.background.color = new Color(0.2f, 0.2f, 0.2f);
                }
                else if (!canAfford)
                {
                    slot.background.color = new Color(0.2f, 0.2f, 0.4f); // Blue-ish for no energy
                }
                else
                {
                    // Ready - bright color
                    Color[] colors = {
                        new Color(0.3f, 0.5f, 0.8f),
                        new Color(0.5f, 0.7f, 0.3f),
                        new Color(0.8f, 0.5f, 0.2f)
                    };
                    slot.background.color = colors[i];
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (playerHero == null) return;

            // Q, W, E or 1, 2, 3 for abilities
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Alpha1))
                OnAbilityClicked(0);
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Alpha2))
                OnAbilityClicked(1);
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Alpha3))
                OnAbilityClicked(2);
        }

        private void OnAbilityClicked(int index)
        {
            Debug.Log($"AbilityBarUI: OnAbilityClicked({index})");

            if (playerHero == null)
            {
                Debug.LogWarning("AbilityBarUI: playerHero is null");
                return;
            }

            var slot = slots[index];
            if (slot == null || slot.ability == null)
            {
                Debug.LogWarning($"AbilityBarUI: slot or ability is null for index {index}");
                return;
            }

            Debug.Log($"AbilityBarUI: Ability '{slot.ability.AbilityName}' - Ready: {slot.ability.IsReady}, CanAfford: {slot.ability.CanAfford}, Targeting: {slot.ability.Targeting}");

            if (!slot.ability.IsReady || !slot.ability.CanAfford)
            {
                Debug.Log($"Ability {index} not ready or can't afford");
                return;
            }

            // For instant abilities, use immediately
            if (slot.ability.Targeting == AbilityBase.TargetingType.Instant)
            {
                Debug.Log($"[ABILITY BAR] Player pressed INSTANT ability '{slot.ability.AbilityName}' - calling playerHero.UseAbility on {playerHero.gameObject.name}");
                playerHero.UseAbility(index, null, null);
            }
            else
            {
                // Start targeting mode
                Debug.Log($"AbilityBarUI: Starting targeting for {slot.ability.AbilityName}");
                GameHUD.Instance?.StartAbilityTargeting(index, slot.ability);
            }
        }

        // Drag-to-aim support
        private bool isDragging;
        private int draggingIndex;
        private Vector2 dragStartPos;

        private void OnPointerDown(int index, PointerEventData data)
        {
            dragStartPos = data.position;
            draggingIndex = index;
        }

        private void OnDrag(int index, PointerEventData data)
        {
            float dragDist = Vector2.Distance(dragStartPos, data.position);
            if (dragDist > 30f && !isDragging)
            {
                isDragging = true;
                var slot = slots[index];
                if (slot?.ability != null && slot.ability.IsReady && slot.ability.CanAfford)
                {
                    GameHUD.Instance?.StartAbilityTargeting(index, slot.ability);
                }
            }
        }

        private void OnPointerUp(int index, PointerEventData data)
        {
            if (isDragging)
            {
                // Confirm ability at drag position
                Vector3? worldPos = GetWorldPosition(data.position);
                if (worldPos.HasValue && playerHero != null)
                {
                    playerHero.UseAbility(index, worldPos, null);
                }
                GameHUD.Instance?.CancelAbilityTargeting();
            }
            isDragging = false;
        }

        private Vector3? GetWorldPosition(Vector2 screenPos)
        {
            if (Camera.main == null) return null;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);

            return null;
        }
    }
}
