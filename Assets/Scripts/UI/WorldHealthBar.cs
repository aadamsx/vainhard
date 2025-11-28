using UnityEngine;
using UnityEngine.UI;
using VaingloryMoba.Characters;

namespace VaingloryMoba.UI
{
    /// <summary>
    /// Health bar that floats above units in world space.
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 2.8f, 0);
        [SerializeField] private float width = 2f;
        [SerializeField] private float height = 0.25f;

        [Header("Colors")]
        [SerializeField] private Color allyHealthColor = new Color(0.1f, 1f, 0.1f);
        [SerializeField] private Color enemyHealthColor = new Color(1f, 0.1f, 0.1f);
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Components
        private Transform target;
        private CharacterStats targetStats;
        private SpriteRenderer background;
        private SpriteRenderer healthFill;
        private Camera mainCamera;
        private bool isEnemy;

        public void Initialize(Transform target, bool isEnemy)
        {
            this.target = target;
            this.isEnemy = isEnemy;
            mainCamera = Camera.main;

            targetStats = target.GetComponent<CharacterStats>();
            if (targetStats == null)
            {
                targetStats = target.GetComponentInChildren<CharacterStats>();
            }

            CreateHealthBar();

            if (targetStats != null)
            {
                targetStats.OnHealthChanged.AddListener(OnHealthChanged);
                OnHealthChanged(targetStats.CurrentHealth, targetStats.MaxHealth);
            }
        }

        private void CreateHealthBar()
        {
            // Create background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform);
            bgObj.transform.localPosition = Vector3.zero;
            background = bgObj.AddComponent<SpriteRenderer>();
            background.sprite = CreateRectSprite();
            background.color = backgroundColor;
            background.sortingOrder = 100;
            bgObj.transform.localScale = new Vector3(width, height, 1);

            // Create health fill
            var fillObj = new GameObject("HealthFill");
            fillObj.transform.SetParent(transform);
            fillObj.transform.localPosition = Vector3.zero;
            healthFill = fillObj.AddComponent<SpriteRenderer>();
            healthFill.sprite = CreateRectSprite();
            healthFill.color = isEnemy ? enemyHealthColor : allyHealthColor;
            healthFill.sortingOrder = 101;
            fillObj.transform.localScale = new Vector3(width, height * 0.8f, 1);
        }

        private Sprite CreateRectSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            // Position above target
            transform.position = target.position + offset;

            // Billboard - face camera
            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            if (healthFill == null) return;

            float percent = current / max;
            healthFill.transform.localScale = new Vector3(width * percent, height * 0.8f, 1);

            // Offset to left-align the health bar fill
            float xOffset = (width - width * percent) * -0.5f;
            healthFill.transform.localPosition = new Vector3(xOffset, 0, 0);
        }

        private void OnDestroy()
        {
            if (targetStats != null)
            {
                targetStats.OnHealthChanged.RemoveListener(OnHealthChanged);
            }
        }
    }
}
