using UnityEngine;
using UnityEngine.UI;
using System;
using VaingloryMoba.Core;

namespace VaingloryMoba.UI
{
    using UnityEngine.EventSystems;
    /// <summary>
    /// Hero selection screen shown before game starts.
    /// </summary>
    public class HeroSelectUI : MonoBehaviour
    {
        public static HeroSelectUI Instance { get; private set; }

        public enum HeroType { Ringo, Taka, Krul }

        public static HeroType SelectedHero { get; private set; } = HeroType.Ringo;
        public static event Action<HeroType> OnHeroSelected;

        private GameObject selectPanel;
        private Button[] heroButtons = new Button[3];
        private Text heroNameText;
        private Text heroDescText;
        private Text[] abilityTexts = new Text[3];
        private Button startButton;
        private bool isShowing = true;

        // Hero data
        private readonly HeroData[] heroes = new HeroData[]
        {
            new HeroData
            {
                type = HeroType.Ringo,
                name = "RINGO",
                role = "Lane Carry",
                description = "A deadly gunslinger who dominates the lane with consistent damage and powerful abilities.",
                color = new Color(0.9f, 0.6f, 0.2f),
                abilities = new string[]
                {
                    "Achilles Shot - Slows target",
                    "Twirling Silver - Attack speed buff",
                    "Hellfire Brew - Homing fireball"
                }
            },
            new HeroData
            {
                type = HeroType.Taka,
                name = "TAKA",
                role = "Jungle Assassin",
                description = "A stealthy ninja who strikes from the shadows and vanishes before enemies can react.",
                color = new Color(0.7f, 0.2f, 0.2f),
                abilities = new string[]
                {
                    "Kaiten - Dodge and flip",
                    "Kaku - Stealth and heal",
                    "X-Retsu - Blink strike burst"
                }
            },
            new HeroData
            {
                type = HeroType.Krul,
                name = "KRUL",
                role = "Jungle Warrior",
                description = "An undead warrior who grows stronger the longer he fights, draining life from his enemies.",
                color = new Color(0.3f, 0.7f, 0.4f),
                abilities = new string[]
                {
                    "Dead Man's Rush - Gap closer",
                    "Spectral Smite - Lifesteal + stacks",
                    "From Hell's Heart - Stun + pull"
                }
            }
        };

        private struct HeroData
        {
            public HeroType type;
            public string name;
            public string role;
            public string description;
            public Color color;
            public string[] abilities;
        }

        private int selectedIndex = 0;

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
            Debug.Log("[HeroSelectUI] Start called, creating UI...");
            CreateUI();
            SelectHero(0);
            Debug.Log($"[HeroSelectUI] UI created. startButton is null: {startButton == null}");
        }

        private void CreateUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Full screen panel
            selectPanel = new GameObject("HeroSelectPanel");
            selectPanel.transform.SetParent(canvas.transform, false);
            var panelRect = selectPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            var panelImage = selectPanel.AddComponent<Image>();
            panelImage.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            // Title
            CreateText(selectPanel.transform, "Title", "SELECT YOUR HERO", 32, new Color(1f, 0.85f, 0.3f),
                new Vector2(0.5f, 1), new Vector2(0, -30));

            // Hero buttons container
            var buttonContainer = new GameObject("HeroButtons");
            buttonContainer.transform.SetParent(selectPanel.transform, false);
            var btnContainerRect = buttonContainer.AddComponent<RectTransform>();
            btnContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnContainerRect.sizeDelta = new Vector2(500, 150);
            btnContainerRect.anchoredPosition = new Vector2(0, 80);

            var layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Create hero buttons
            for (int i = 0; i < heroes.Length; i++)
            {
                CreateHeroButton(buttonContainer.transform, i);
            }

            // Hero info panel
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(selectPanel.transform, false);
            var infoRect = infoPanel.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.5f, 0.5f);
            infoRect.anchorMax = new Vector2(0.5f, 0.5f);
            infoRect.sizeDelta = new Vector2(450, 200);
            infoRect.anchoredPosition = new Vector2(0, -80);

            var infoImage = infoPanel.AddComponent<Image>();
            infoImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Hero name
            var nameObj = CreateText(infoPanel.transform, "HeroName", "RINGO", 24, Color.white,
                new Vector2(0.5f, 1), new Vector2(0, -15));
            heroNameText = nameObj.GetComponent<Text>();

            // Hero description
            var descObj = CreateText(infoPanel.transform, "HeroDesc", "", 14, new Color(0.8f, 0.8f, 0.8f),
                new Vector2(0.5f, 1), new Vector2(0, -50));
            heroDescText = descObj.GetComponent<Text>();
            var descRect = descObj.GetComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(400, 40);

            // Abilities
            for (int i = 0; i < 3; i++)
            {
                var abilityObj = CreateText(infoPanel.transform, $"Ability{i}", "", 12, new Color(0.7f, 0.9f, 1f),
                    new Vector2(0.5f, 1), new Vector2(0, -100 - (i * 22)));
                abilityTexts[i] = abilityObj.GetComponent<Text>();
            }

            // Start button
            var startObj = new GameObject("StartButton");
            startObj.transform.SetParent(selectPanel.transform, false);
            var startRect = startObj.AddComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.5f, 0);
            startRect.anchorMax = new Vector2(0.5f, 0);
            startRect.pivot = new Vector2(0.5f, 0);
            startRect.anchoredPosition = new Vector2(0, 50);
            startRect.sizeDelta = new Vector2(200, 60);

            var startImage = startObj.AddComponent<Image>();
            startImage.color = new Color(0.2f, 0.6f, 0.2f);

            startButton = startObj.AddComponent<Button>();
            startButton.onClick.AddListener(OnStartClicked);

            var startTextObj = CreateText(startObj.transform, "Text", "START GAME", 20, Color.white,
                new Vector2(0.5f, 0.5f), Vector2.zero);
        }

        private void CreateHeroButton(Transform parent, int index)
        {
            var hero = heroes[index];

            var btnObj = new GameObject(hero.name);
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140, 140);

            // Add LayoutElement to enforce size in HorizontalLayoutGroup
            var layoutElement = btnObj.AddComponent<LayoutElement>();
            layoutElement.minWidth = 140;
            layoutElement.minHeight = 140;
            layoutElement.preferredWidth = 140;
            layoutElement.preferredHeight = 140;

            var image = btnObj.AddComponent<Image>();
            image.color = hero.color * 0.5f;

            var btn = btnObj.AddComponent<Button>();
            int idx = index;
            btn.onClick.AddListener(() => SelectHero(idx));
            heroButtons[index] = btn;

            // Hero icon placeholder (colored square with letter)
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(10, 30);
            iconRect.offsetMax = new Vector2(-10, -10);

            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = hero.color;

            // Letter
            var letterObj = CreateText(iconObj.transform, "Letter", hero.name.Substring(0, 1), 48, Color.white,
                new Vector2(0.5f, 0.5f), Vector2.zero);

            // Name below
            var nameObj = CreateText(btnObj.transform, "Name", hero.name, 12, Color.white,
                new Vector2(0.5f, 0), new Vector2(0, 8));

            // Role
            var roleObj = CreateText(btnObj.transform, "Role", hero.role, 9, new Color(0.7f, 0.7f, 0.7f),
                new Vector2(0.5f, 0), new Vector2(0, -5));
        }

        private GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color,
            Vector2 anchor, Vector2 position)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300, 30);

            var text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;

            return obj;
        }

        private void SelectHero(int index)
        {
            selectedIndex = index;
            var hero = heroes[index];

            // Update button highlights
            for (int i = 0; i < heroButtons.Length; i++)
            {
                if (heroButtons[i] != null)
                {
                    var img = heroButtons[i].GetComponent<Image>();
                    img.color = i == index ? heroes[i].color : heroes[i].color * 0.4f;
                }
            }

            // Update info panel
            if (heroNameText != null)
            {
                heroNameText.text = hero.name;
                heroNameText.color = hero.color;
            }

            if (heroDescText != null)
            {
                heroDescText.text = hero.description;
            }

            for (int i = 0; i < 3; i++)
            {
                if (abilityTexts[i] != null)
                {
                    abilityTexts[i].text = hero.abilities[i];
                }
            }

            SelectedHero = hero.type;
        }

        private void OnStartClicked()
        {
            Debug.Log($"[HeroSelectUI] OnStartClicked called! Hero: {SelectedHero}");

            // Notify listeners
            OnHeroSelected?.Invoke(SelectedHero);

            // Hide selection screen
            if (selectPanel != null)
            {
                selectPanel.SetActive(false);
            }
            isShowing = false;

            // Tell GameManager to start
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGameWithHero(SelectedHero);
            }
        }

        public bool IsShowing => isShowing;
    }
}
