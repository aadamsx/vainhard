using UnityEngine;
using UnityEngine.UI;

namespace VaingloryMoba.UI
{
    /// <summary>
    /// Floating damage number that rises and fades out.
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float riseSpeed = 2f;
        [SerializeField] private float fadeStartTime = 0.5f;

        private Text textComponent;
        private float spawnTime;
        private Vector3 worldPosition;
        private Color startColor;

        public static void Spawn(Vector3 position, float damage, bool isCrit = false, bool isHeal = false)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            var obj = new GameObject("DamageNumber");
            obj.transform.SetParent(canvas.transform);

            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 50);

            var text = obj.AddComponent<Text>();
            text.text = Mathf.RoundToInt(damage).ToString();
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (isHeal)
            {
                text.color = new Color(0.3f, 1f, 0.3f); // Green for heals
                text.text = "+" + text.text;
                text.fontSize = 18;
            }
            else if (isCrit)
            {
                text.color = new Color(1f, 0.8f, 0.2f); // Gold for crits
                text.text = text.text + "!";
                text.fontSize = 26;
            }
            else
            {
                text.color = Color.white;
                text.fontSize = 20;
            }

            var dmgNum = obj.AddComponent<DamageNumber>();
            dmgNum.worldPosition = position + Vector3.up * 2f;
            dmgNum.textComponent = text;
            dmgNum.startColor = text.color;
            dmgNum.spawnTime = Time.time;

            // Add slight random offset
            dmgNum.worldPosition += new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        }

        private void Update()
        {
            float elapsed = Time.time - spawnTime;

            // Rise upward
            worldPosition += Vector3.up * riseSpeed * Time.deltaTime;

            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            transform.position = screenPos;

            // Fade out
            if (elapsed > fadeStartTime)
            {
                float fadeProgress = (elapsed - fadeStartTime) / (lifetime - fadeStartTime);
                Color c = startColor;
                c.a = 1f - fadeProgress;
                textComponent.color = c;
            }

            // Destroy when done
            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }

            // Hide if behind camera
            if (screenPos.z < 0)
            {
                textComponent.enabled = false;
            }
            else
            {
                textComponent.enabled = true;
            }
        }
    }
}
