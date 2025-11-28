using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using VaingloryMoba.Combat;
using VaingloryMoba.Characters;

namespace VaingloryMoba.UI
{
    /// <summary>
    /// Vainglory-style shop with category tabs and tiered items.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance { get; private set; }

        // UI References
        private GameObject shopPanel;
        private Transform itemGridParent;
        private Text goldText;
        private Text selectedItemName;
        private Text selectedItemDescription;
        private Text selectedItemCost;
        private Button buyButton;
        private List<Button> categoryButtons = new List<Button>();

        // State
        private Inventory playerInventory;
        private CharacterStats playerStats;
        private List<GameObject> itemButtons = new List<GameObject>();
        private bool isOpen;
        private ItemCategory currentCategory = ItemCategory.Weapon;
        private ItemData? selectedItemData;
        private List<ItemData> purchasedItems = new List<ItemData>();
        private int itemPurchaseCount = 0;

        public bool IsOpen => isOpen;

        // Item categories
        public enum ItemCategory { Weapon, Crystal, Defense, Utility }

        // Item data structure
        public struct ItemData
        {
            public string name;
            public int cost;
            public int tier; // 1, 2, or 3
            public ItemCategory category;
            public float weaponPower;
            public float crystalPower;
            public float maxHealth;
            public float armor;
            public float shield;
            public float attackSpeed;
            public float cooldownReduction;
            public float lifesteal;
            public float moveSpeed;
            public float critChance;         // 0.2 = 20% crit chance
            public float critDamage;         // 0.25 = +25% crit damage
            public float armorPierce;        // Flat armor pierce
            public float armorPiercePercent; // 0.1 = 10% armor pierce
            public float shieldPierce;       // Flat shield pierce
            public float shieldPiercePercent;// 0.1 = 10% shield pierce
            public string passive;
            public Color color;
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
            CreateShopUI();
            StartCoroutine(WaitForPlayer());
        }

        private void CreateShopUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Shop button (bottom right)
            CreateShopButton(canvas);

            // Main shop panel
            shopPanel = new GameObject("ShopPanel");
            shopPanel.transform.SetParent(canvas.transform, false);
            var panelRect = shopPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700, 550);

            var panelImage = shopPanel.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

            // Title
            CreateText(shopPanel.transform, "Title", "SHOP", 28, new Color(1f, 0.85f, 0.3f),
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), new Vector2(0, 40));

            // Gold display
            var goldObj = CreateText(shopPanel.transform, "Gold", "Gold: 500", 20, new Color(1f, 0.85f, 0.3f),
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -15), new Vector2(150, 30));
            goldText = goldObj.GetComponent<Text>();
            goldText.alignment = TextAnchor.MiddleRight;

            // Close button
            CreateCloseButton();

            // Category tabs
            CreateCategoryTabs();

            // Item grid with scroll
            CreateItemGrid();

            // Info panel (right side)
            CreateInfoPanel();

            // Start hidden
            shopPanel.SetActive(false);
            isOpen = false;

            // Populate initial category
            PopulateItemGrid(currentCategory);

            Debug.Log("Shop UI created with all items");
        }

        private void CreateShopButton(Canvas canvas)
        {
            var btn = CreateButton(canvas.transform, "ShopButton", "SHOP", new Color(0.6f, 0.5f, 0.2f),
                new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 20), new Vector2(80, 40));
            btn.onClick.AddListener(ToggleShop);
        }

        private void CreateCloseButton()
        {
            var closeBtn = CreateButton(shopPanel.transform, "CloseButton", "X", new Color(0.7f, 0.2f, 0.2f),
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(30, 30));
            closeBtn.onClick.AddListener(CloseShop);
        }

        private void CreateCategoryTabs()
        {
            var tabContainer = new GameObject("TabContainer");
            tabContainer.transform.SetParent(shopPanel.transform, false);
            var tabRect = tabContainer.AddComponent<RectTransform>();
            tabRect.anchorMin = new Vector2(0, 1);
            tabRect.anchorMax = new Vector2(0.65f, 1);
            tabRect.offsetMin = new Vector2(10, -90);
            tabRect.offsetMax = new Vector2(-10, -50);

            var layout = tabContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 5;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            string[] categories = { "WEAPON", "CRYSTAL", "DEFENSE", "UTILITY" };
            Color[] colors = {
                new Color(0.8f, 0.4f, 0.2f), // Orange for weapon
                new Color(0.5f, 0.3f, 0.8f), // Purple for crystal
                new Color(0.3f, 0.6f, 0.3f), // Green for defense
                new Color(0.3f, 0.5f, 0.7f)  // Blue for utility
            };

            for (int i = 0; i < categories.Length; i++)
            {
                var btnObj = new GameObject(categories[i]);
                btnObj.transform.SetParent(tabContainer.transform, false);

                var img = btnObj.AddComponent<Image>();
                img.color = i == 0 ? colors[i] : new Color(0.25f, 0.25f, 0.3f);

                var btn = btnObj.AddComponent<Button>();
                int index = i;
                btn.onClick.AddListener(() => SelectCategory((ItemCategory)index));
                categoryButtons.Add(btn);

                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                var text = textObj.AddComponent<Text>();
                text.text = categories[i];
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 12;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private void CreateItemGrid()
        {
            // Simple grid container (no scroll for now - ensure items show)
            var gridContainer = new GameObject("ItemGrid");
            gridContainer.transform.SetParent(shopPanel.transform, false);
            var gridRect = gridContainer.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 0);
            gridRect.anchorMax = new Vector2(0.65f, 1);
            gridRect.offsetMin = new Vector2(10, 60);
            gridRect.offsetMax = new Vector2(-10, -95);

            var gridImage = gridContainer.AddComponent<Image>();
            gridImage.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            // Content with grid layout
            var content = new GameObject("Content");
            content.transform.SetParent(gridContainer.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            itemGridParent = content.transform;

            var gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(95, 70);
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
            gridLayout.padding = new RectOffset(8, 8, 8, 8);
        }

        private void CreateInfoPanel()
        {
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(shopPanel.transform, false);
            var infoRect = infoPanel.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.65f, 0);
            infoRect.anchorMax = new Vector2(1, 1);
            infoRect.offsetMin = new Vector2(5, 10);
            infoRect.offsetMax = new Vector2(-10, -50);

            var infoImage = infoPanel.AddComponent<Image>();
            infoImage.color = new Color(0.1f, 0.1f, 0.14f, 0.95f);

            // Item name
            var nameObj = CreateText(infoPanel.transform, "ItemName", "Select an item", 16, Color.white,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), new Vector2(-10, 28));
            selectedItemName = nameObj.GetComponent<Text>();
            selectedItemName.alignment = TextAnchor.MiddleCenter;

            // Item description
            var descObj = CreateText(infoPanel.transform, "ItemDesc", "", 12, new Color(0.75f, 0.75f, 0.75f),
                new Vector2(0, 0.35f), new Vector2(1, 1), new Vector2(0, -45), new Vector2(-10, -50));
            selectedItemDescription = descObj.GetComponent<Text>();
            selectedItemDescription.alignment = TextAnchor.UpperCenter;

            // Item cost
            var costObj = CreateText(infoPanel.transform, "ItemCost", "", 18, new Color(1f, 0.85f, 0.3f),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 55), new Vector2(-10, 25));
            selectedItemCost = costObj.GetComponent<Text>();
            selectedItemCost.alignment = TextAnchor.MiddleCenter;

            // Buy button
            buyButton = CreateButton(infoPanel.transform, "BuyButton", "BUY", new Color(0.2f, 0.55f, 0.2f),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 15), new Vector2(120, 35));
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        private Button CreateButton(Transform parent, string name, string text, Color bgColor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            var btn = btnObj.AddComponent<Button>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = 14;
            textComp.color = Color.white;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return btn;
        }

        private GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;

            return obj;
        }

        private void SelectCategory(ItemCategory category)
        {
            currentCategory = category;

            // Update tab colors
            Color[] colors = {
                new Color(0.8f, 0.4f, 0.2f),
                new Color(0.5f, 0.3f, 0.8f),
                new Color(0.3f, 0.6f, 0.3f),
                new Color(0.3f, 0.5f, 0.7f)
            };

            for (int i = 0; i < categoryButtons.Count; i++)
            {
                var img = categoryButtons[i].GetComponent<Image>();
                img.color = i == (int)category ? colors[i] : new Color(0.25f, 0.25f, 0.3f);
            }

            PopulateItemGrid(category);
        }

        private void PopulateItemGrid(ItemCategory category)
        {
            // Clear existing
            foreach (var btn in itemButtons)
            {
                if (btn != null) Destroy(btn);
            }
            itemButtons.Clear();

            if (itemGridParent == null)
            {
                Debug.LogError("ShopUI: itemGridParent is null!");
                return;
            }

            var items = GetItemsForCategory(category);
            Debug.Log($"ShopUI: Populating {items.Length} items for category {category}");

            foreach (var item in items)
            {
                CreateItemButton(item);
            }

            Debug.Log($"ShopUI: Created {itemButtons.Count} item buttons, parent has {itemGridParent.childCount} children");
        }

        private void CreateItemButton(ItemData item)
        {
            var btnObj = new GameObject(item.name);
            btnObj.transform.SetParent(itemGridParent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(95, 70);

            var img = btnObj.AddComponent<Image>();
            // Color by tier - brighter so visible
            float tierBrightness = 0.3f + (item.tier * 0.15f);
            img.color = new Color(
                Mathf.Clamp01(item.color.r * tierBrightness + 0.1f),
                Mathf.Clamp01(item.color.g * tierBrightness + 0.1f),
                Mathf.Clamp01(item.color.b * tierBrightness + 0.1f));

            var btn = btnObj.AddComponent<Button>();
            var itemCopy = item;
            btn.onClick.AddListener(() => SelectItem(itemCopy));

            // Tier indicator (stars or roman numerals)
            var tierObj = new GameObject("Tier");
            tierObj.transform.SetParent(btnObj.transform, false);
            var tierRect = tierObj.AddComponent<RectTransform>();
            tierRect.anchorMin = new Vector2(0, 1);
            tierRect.anchorMax = new Vector2(0, 1);
            tierRect.pivot = new Vector2(0, 1);
            tierRect.anchoredPosition = new Vector2(2, -2);
            tierRect.sizeDelta = new Vector2(20, 14);
            var tierText = tierObj.AddComponent<Text>();
            tierText.text = item.tier == 1 ? "T1" : item.tier == 2 ? "T2" : "T3";
            tierText.fontSize = 9;
            tierText.color = item.tier == 3 ? new Color(1f, 0.85f, 0.3f) : Color.white;
            tierText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Item name
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(2, 14);
            textRect.offsetMax = new Vector2(-2, -2);
            var text = textObj.AddComponent<Text>();
            text.text = item.name;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 10;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Cost
            var costObj = new GameObject("Cost");
            costObj.transform.SetParent(btnObj.transform, false);
            var costRect = costObj.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 0);
            costRect.anchorMax = new Vector2(1, 0);
            costRect.pivot = new Vector2(0.5f, 0);
            costRect.anchoredPosition = new Vector2(0, 2);
            costRect.sizeDelta = new Vector2(0, 12);
            var costText = costObj.AddComponent<Text>();
            costText.text = $"{item.cost}g";
            costText.alignment = TextAnchor.MiddleCenter;
            costText.fontSize = 9;
            costText.color = new Color(1f, 0.85f, 0.3f);
            costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            itemButtons.Add(btnObj);
        }

        private ItemData[] GetItemsForCategory(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Weapon:
                    return GetWeaponItems();
                case ItemCategory.Crystal:
                    return GetCrystalItems();
                case ItemCategory.Defense:
                    return GetDefenseItems();
                case ItemCategory.Utility:
                    return GetUtilityItems();
                default:
                    return new ItemData[0];
            }
        }

        private ItemData[] GetWeaponItems()
        {
            Color wpColor = new Color(1f, 0.5f, 0.2f);
            return new ItemData[]
            {
                // Tier 1
                new ItemData { name = "Weapon Blade", cost = 300, tier = 1, category = ItemCategory.Weapon,
                    weaponPower = 15, color = wpColor },
                new ItemData { name = "Book of Eulogies", cost = 300, tier = 1, category = ItemCategory.Weapon,
                    weaponPower = 5, lifesteal = 0.1f, color = wpColor },
                new ItemData { name = "Swift Shooter", cost = 300, tier = 1, category = ItemCategory.Weapon,
                    attackSpeed = 0.2f, color = wpColor },

                // Tier 2
                new ItemData { name = "Heavy Steel", cost = 1150, tier = 2, category = ItemCategory.Weapon,
                    weaponPower = 55, color = wpColor },
                new ItemData { name = "Six Sins", cost = 650, tier = 2, category = ItemCategory.Weapon,
                    weaponPower = 30, color = wpColor },
                new ItemData { name = "Barbed Needle", cost = 800, tier = 2, category = ItemCategory.Weapon,
                    weaponPower = 15, lifesteal = 0.1f, color = wpColor },
                new ItemData { name = "Blazing Salvo", cost = 700, tier = 2, category = ItemCategory.Weapon,
                    attackSpeed = 0.35f, color = wpColor },
                new ItemData { name = "Piercing Spear", cost = 900, tier = 2, category = ItemCategory.Weapon,
                    weaponPower = 15, armorPiercePercent = 0.1f, passive = "10% Armor Pierce", color = wpColor },
                new ItemData { name = "Lucky Strike", cost = 900, tier = 2, category = ItemCategory.Weapon,
                    weaponPower = 20, critChance = 0.2f, passive = "20% Crit Chance", color = wpColor },

                // Tier 3
                new ItemData { name = "Sorrowblade", cost = 3100, tier = 3, category = ItemCategory.Weapon,
                    weaponPower = 150, color = wpColor },
                new ItemData { name = "Serpent Mask", cost = 2800, tier = 3, category = ItemCategory.Weapon,
                    weaponPower = 85, lifesteal = 0.2f, passive = "+20% Lifesteal", color = wpColor },
                new ItemData { name = "Poisoned Shiv", cost = 2250, tier = 3, category = ItemCategory.Weapon,
                    weaponPower = 35, attackSpeed = 0.4f, lifesteal = 0.1f, passive = "Mortal Wounds", color = wpColor },
                new ItemData { name = "Breaking Point", cost = 2600, tier = 3, category = ItemCategory.Weapon,
                    weaponPower = 55, attackSpeed = 0.35f, passive = "Stacking damage", color = wpColor },
                new ItemData { name = "Tornado Trigger", cost = 2600, tier = 3, category = ItemCategory.Weapon,
                    attackSpeed = 0.75f, critChance = 0.35f, passive = "35% Crit Chance", color = wpColor },
                new ItemData { name = "Tyrant's Monocle", cost = 2750, tier = 3, category = ItemCategory.Weapon,
                    weaponPower = 50, critChance = 0.4f, critDamage = 0.25f, passive = "40% Crit, +25% Crit Dmg", color = wpColor },
                new ItemData { name = "Tension Bow", cost = 2150, tier = 3, category = ItemCategory.Weapon,
                    weaponPower = 45, armorPierce = 10f, passive = "180 bonus dmg (6s)", color = wpColor },
                new ItemData { name = "Bonesaw", cost = 2700, tier = 3, category = ItemCategory.Weapon,
                    weaponPower = 25, attackSpeed = 0.5f, armorPiercePercent = 0.2f, passive = "Shred 20% armor", color = wpColor },
            };
        }

        private ItemData[] GetCrystalItems()
        {
            Color cpColor = new Color(0.6f, 0.3f, 0.9f);
            return new ItemData[]
            {
                // Tier 1
                new ItemData { name = "Crystal Bit", cost = 300, tier = 1, category = ItemCategory.Crystal,
                    crystalPower = 20, color = cpColor },
                new ItemData { name = "Energy Battery", cost = 300, tier = 1, category = ItemCategory.Crystal,
                    crystalPower = 5, passive = "+150 Energy", color = cpColor },
                new ItemData { name = "Hourglass", cost = 250, tier = 1, category = ItemCategory.Crystal,
                    cooldownReduction = 0.1f, color = cpColor },

                // Tier 2
                new ItemData { name = "Heavy Prism", cost = 1050, tier = 2, category = ItemCategory.Crystal,
                    crystalPower = 50, color = cpColor },
                new ItemData { name = "Eclipse Prism", cost = 650, tier = 2, category = ItemCategory.Crystal,
                    crystalPower = 35, color = cpColor },
                new ItemData { name = "Piercing Shard", cost = 900, tier = 2, category = ItemCategory.Crystal,
                    crystalPower = 20, shieldPiercePercent = 0.1f, passive = "10% Shield Pierce", color = cpColor },
                new ItemData { name = "Chronograph", cost = 800, tier = 2, category = ItemCategory.Crystal,
                    crystalPower = 10, cooldownReduction = 0.2f, color = cpColor },
                new ItemData { name = "Void Battery", cost = 700, tier = 2, category = ItemCategory.Crystal,
                    crystalPower = 25, passive = "+250 Energy", color = cpColor },

                // Tier 3
                new ItemData { name = "Shatterglass", cost = 3000, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 150, color = cpColor },
                new ItemData { name = "Spellfire", cost = 2400, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 100, passive = "Mortal Wounds", color = cpColor },
                new ItemData { name = "Frostburn", cost = 2600, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 100, passive = "Slow enemies 30%", color = cpColor },
                new ItemData { name = "Eve of Harvest", cost = 2600, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 55, lifesteal = 0.15f, passive = "15% Crystal Lifesteal", color = cpColor },
                new ItemData { name = "Broken Myth", cost = 2150, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 70, shieldPiercePercent = 0.3f, passive = "30% Shield Pierce", color = cpColor },
                new ItemData { name = "Clockwork", cost = 2500, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 30, cooldownReduction = 0.35f, passive = "Energy regen", color = cpColor },
                new ItemData { name = "Alternating Current", cost = 2800, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 60, attackSpeed = 0.65f, passive = "CP basic attack", color = cpColor },
                new ItemData { name = "Dragon's Eye", cost = 2800, tier = 3, category = ItemCategory.Crystal,
                    crystalPower = 85, passive = "Stacking CP on hit", color = cpColor },
            };
        }

        private ItemData[] GetDefenseItems()
        {
            Color defColor = new Color(0.3f, 0.7f, 0.3f);
            return new ItemData[]
            {
                // Tier 1
                new ItemData { name = "Oakheart", cost = 300, tier = 1, category = ItemCategory.Defense,
                    maxHealth = 250, color = defColor },
                new ItemData { name = "Light Armor", cost = 300, tier = 1, category = ItemCategory.Defense,
                    armor = 30, color = defColor },
                new ItemData { name = "Light Shield", cost = 300, tier = 1, category = ItemCategory.Defense,
                    shield = 30, color = defColor },
                new ItemData { name = "Reflex Block", cost = 700, tier = 1, category = ItemCategory.Defense,
                    passive = "Block ability", color = defColor },

                // Tier 2
                new ItemData { name = "Dragonheart", cost = 650, tier = 2, category = ItemCategory.Defense,
                    maxHealth = 500, color = defColor },
                new ItemData { name = "Coat of Plates", cost = 800, tier = 2, category = ItemCategory.Defense,
                    armor = 70, color = defColor },
                new ItemData { name = "Kinetic Shield", cost = 800, tier = 2, category = ItemCategory.Defense,
                    shield = 70, color = defColor },
                new ItemData { name = "Lifespring", cost = 800, tier = 2, category = ItemCategory.Defense,
                    maxHealth = 250, passive = "Regen out of combat", color = defColor },

                // Tier 3
                new ItemData { name = "Slumbering Husk", cost = 1600, tier = 3, category = ItemCategory.Defense,
                    maxHealth = 400, armor = 30, shield = 30, passive = "Anti-burst", color = defColor },
                new ItemData { name = "Metal Jacket", cost = 2100, tier = 3, category = ItemCategory.Defense,
                    armor = 120, passive = "Anti-WP carry", color = defColor },
                new ItemData { name = "Aegis", cost = 2100, tier = 3, category = ItemCategory.Defense,
                    shield = 85, passive = "Reflex Block", color = defColor },
                new ItemData { name = "Atlas Pauldron", cost = 1900, tier = 3, category = ItemCategory.Defense,
                    armor = 85, passive = "Slow atk speed", color = defColor },
                new ItemData { name = "Crucible", cost = 1850, tier = 3, category = ItemCategory.Defense,
                    maxHealth = 600, passive = "Team Reflex", color = defColor },
                new ItemData { name = "Fountain of Renewal", cost = 2300, tier = 3, category = ItemCategory.Defense,
                    maxHealth = 300, armor = 30, shield = 30, passive = "Team heal", color = defColor },
                new ItemData { name = "Pulseweave", cost = 2100, tier = 3, category = ItemCategory.Defense,
                    maxHealth = 500, armor = 35, passive = "Slow nearby", color = defColor },
                new ItemData { name = "Rook's Decree", cost = 2200, tier = 3, category = ItemCategory.Defense,
                    maxHealth = 400, armor = 50, passive = "Ally barrier", color = defColor },
            };
        }

        private ItemData[] GetUtilityItems()
        {
            Color utilColor = new Color(0.3f, 0.5f, 0.8f);
            return new ItemData[]
            {
                // Tier 1
                new ItemData { name = "Sprint Boots", cost = 300, tier = 1, category = ItemCategory.Utility,
                    moveSpeed = 0.5f, color = utilColor },
                new ItemData { name = "Minion's Foot", cost = 300, tier = 1, category = ItemCategory.Utility,
                    passive = "+15% Minion dmg", color = utilColor },
                new ItemData { name = "Flare", cost = 25, tier = 1, category = ItemCategory.Utility,
                    passive = "Vision (consumable)", color = utilColor },
                new ItemData { name = "Scout Trap", cost = 50, tier = 1, category = ItemCategory.Utility,
                    passive = "Hidden trap", color = utilColor },

                // Tier 2
                new ItemData { name = "Travel Boots", cost = 500, tier = 2, category = ItemCategory.Utility,
                    moveSpeed = 0.6f, passive = "Sprint", color = utilColor },
                new ItemData { name = "Stormguard Banner", cost = 1100, tier = 2, category = ItemCategory.Utility,
                    maxHealth = 150, passive = "True dmg to minions", color = utilColor },
                new ItemData { name = "Contraption", cost = 1650, tier = 2, category = ItemCategory.Utility,
                    maxHealth = 350, cooldownReduction = 0.25f, passive = "Traps & Flares", color = utilColor },

                // Tier 3
                new ItemData { name = "Journey Boots", cost = 1900, tier = 3, category = ItemCategory.Utility,
                    moveSpeed = 0.6f, passive = "Sprint on hero dmg", color = utilColor },
                new ItemData { name = "War Treads", cost = 2000, tier = 3, category = ItemCategory.Utility,
                    maxHealth = 500, moveSpeed = 0.5f, passive = "Team sprint", color = utilColor },
                new ItemData { name = "Halcyon Chargers", cost = 2100, tier = 3, category = ItemCategory.Utility,
                    moveSpeed = 0.5f, cooldownReduction = 0.15f, passive = "Energy/cooldown", color = utilColor },
                new ItemData { name = "Teleport Boots", cost = 2000, tier = 3, category = ItemCategory.Utility,
                    moveSpeed = 0.5f, passive = "Teleport to ally", color = utilColor },
                new ItemData { name = "Stormcrown", cost = 2200, tier = 3, category = ItemCategory.Utility,
                    maxHealth = 200, cooldownReduction = 0.3f, passive = "True dmg on-hit", color = utilColor },
                new ItemData { name = "Aftershock", cost = 2400, tier = 3, category = ItemCategory.Utility,
                    crystalPower = 35, cooldownReduction = 0.2f, passive = "On-ability dmg", color = utilColor },
                new ItemData { name = "Echo", cost = 2500, tier = 3, category = ItemCategory.Utility,
                    crystalPower = 40, passive = "Repeat ability", color = utilColor },
                new ItemData { name = "Nullwave Gauntlet", cost = 2250, tier = 3, category = ItemCategory.Utility,
                    maxHealth = 300, passive = "Item silence", color = utilColor },
                new ItemData { name = "Shiversteel", cost = 1450, tier = 3, category = ItemCategory.Utility,
                    maxHealth = 500, passive = "Basic atk slow", color = utilColor },
                new ItemData { name = "Capacitor Plate", cost = 1800, tier = 3, category = ItemCategory.Utility,
                    maxHealth = 250, passive = "Barrier on ability", color = utilColor },
            };
        }

        private void SelectItem(ItemData item)
        {
            selectedItemData = item;

            if (selectedItemName != null)
                selectedItemName.text = item.name;

            if (selectedItemDescription != null)
            {
                string desc = "";
                if (item.weaponPower > 0) desc += $"+{item.weaponPower} Weapon Power\n";
                if (item.crystalPower > 0) desc += $"+{item.crystalPower} Crystal Power\n";
                if (item.maxHealth > 0) desc += $"+{item.maxHealth} Health\n";
                if (item.armor > 0) desc += $"+{item.armor} Armor\n";
                if (item.shield > 0) desc += $"+{item.shield} Shield\n";
                if (item.attackSpeed > 0) desc += $"+{(int)(item.attackSpeed * 100)}% Atk Speed\n";
                if (item.cooldownReduction > 0) desc += $"+{(int)(item.cooldownReduction * 100)}% CDR\n";
                if (item.lifesteal > 0) desc += $"+{(int)(item.lifesteal * 100)}% Lifesteal\n";
                if (item.moveSpeed > 0) desc += $"+{item.moveSpeed} Move Speed\n";
                if (item.critChance > 0) desc += $"+{(int)(item.critChance * 100)}% Crit Chance\n";
                if (item.critDamage > 0) desc += $"+{(int)(item.critDamage * 100)}% Crit Damage\n";
                if (item.armorPierce > 0) desc += $"+{item.armorPierce} Armor Pierce\n";
                if (item.armorPiercePercent > 0) desc += $"+{(int)(item.armorPiercePercent * 100)}% Armor Pierce\n";
                if (item.shieldPierce > 0) desc += $"+{item.shieldPierce} Shield Pierce\n";
                if (item.shieldPiercePercent > 0) desc += $"+{(int)(item.shieldPiercePercent * 100)}% Shield Pierce\n";
                if (!string.IsNullOrEmpty(item.passive)) desc += $"\n{item.passive}";
                selectedItemDescription.text = desc;
            }

            if (selectedItemCost != null)
                selectedItemCost.text = $"{item.cost} Gold";

            UpdateBuyButton();
        }

        private void UpdateBuyButton()
        {
            if (buyButton == null) return;

            bool canBuy = selectedItemData.HasValue &&
                          playerStats != null &&
                          playerStats.Gold >= selectedItemData.Value.cost &&
                          purchasedItems.Count < 6;

            buyButton.interactable = canBuy;
            var img = buyButton.GetComponent<Image>();
            img.color = canBuy ? new Color(0.2f, 0.55f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
        }

        private void OnBuyClicked()
        {
            if (!selectedItemData.HasValue) return;
            if (playerStats == null) return;

            var itemData = selectedItemData.Value;

            if (purchasedItems.Count >= 6)
            {
                Debug.Log("Inventory full!");
                return;
            }

            if (playerStats.SpendGold(itemData.cost))
            {
                itemPurchaseCount++;
                string itemId = $"item_{itemPurchaseCount}_{itemData.name}";
                purchasedItems.Add(itemData);

                Debug.Log($"Purchased {itemData.name} for {itemData.cost} gold. Items: {purchasedItems.Count}/6");

                // Create an Item object and add to inventory
                if (playerInventory != null)
                {
                    var item = ScriptableObject.CreateInstance<Item>();
                    item.itemName = itemData.name;
                    item.cost = itemData.cost;
                    item.tier = itemData.tier;
                    item.weaponPower = itemData.weaponPower;
                    item.crystalPower = itemData.crystalPower;
                    item.maxHealth = itemData.maxHealth;
                    item.armor = itemData.armor;
                    item.shield = itemData.shield;
                    item.attackSpeed = itemData.attackSpeed;
                    item.cooldownReduction = itemData.cooldownReduction;
                    item.lifesteal = itemData.lifesteal;
                    item.moveSpeed = itemData.moveSpeed;
                    item.critChance = itemData.critChance;
                    item.critDamage = itemData.critDamage;
                    item.armorPierce = itemData.armorPierce;
                    item.armorPiercePercent = itemData.armorPiercePercent;
                    item.shieldPierce = itemData.shieldPierce;
                    item.shieldPiercePercent = itemData.shieldPiercePercent;

                    // Add to inventory (inventory handles bonus tracking)
                    playerInventory.AddItemDirectly(item);
                }

                // Also apply flat bonuses to CharacterStats for immediate effect
                if (itemData.weaponPower > 0)
                    playerStats.AddFlatBonus(CharacterStats.StatType.WeaponPower, itemData.weaponPower, itemId);
                if (itemData.crystalPower > 0)
                    playerStats.AddFlatBonus(CharacterStats.StatType.CrystalPower, itemData.crystalPower, itemId);
                if (itemData.maxHealth > 0)
                    playerStats.AddFlatBonus(CharacterStats.StatType.MaxHealth, itemData.maxHealth, itemId);
                if (itemData.armor > 0)
                    playerStats.AddFlatBonus(CharacterStats.StatType.Armor, itemData.armor, itemId);
                if (itemData.shield > 0)
                    playerStats.AddFlatBonus(CharacterStats.StatType.Shield, itemData.shield, itemId);
                if (itemData.attackSpeed > 0)
                    playerStats.ApplyModifier($"{itemId}_as", CharacterStats.StatType.AttackSpeed, 1f + itemData.attackSpeed, -1);

                UpdateGoldDisplay(playerStats.Gold);
                UpdateBuyButton();
            }
        }

        private System.Collections.IEnumerator WaitForPlayer()
        {
            while (Core.GameManager.Instance == null || Core.GameManager.Instance.PlayerHero == null)
                yield return null;

            var hero = Core.GameManager.Instance.PlayerHero;
            playerInventory = hero.GetComponent<Inventory>();
            playerStats = hero.GetComponent<CharacterStats>();

            if (playerInventory == null)
                playerInventory = hero.AddComponent<Inventory>();

            if (playerStats != null)
            {
                playerStats.OnGoldChanged.AddListener(UpdateGoldDisplay);
                UpdateGoldDisplay(playerStats.Gold);
            }
        }

        private void UpdateGoldDisplay(int gold)
        {
            if (goldText != null)
                goldText.text = $"Gold: {gold}";
            UpdateBuyButton();
        }

        public void OpenShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                isOpen = true;
                PopulateItemGrid(currentCategory);
            }
            UpdateGoldDisplay(playerStats != null ? playerStats.Gold : 0);
        }

        public void CloseShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
                isOpen = false;
            }
        }

        public void ToggleShop()
        {
            if (isOpen) CloseShop();
            else OpenShop();
        }

        private void Update()
        {
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
                CloseShop();
        }
    }
}
