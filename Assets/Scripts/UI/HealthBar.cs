using UnityEngine;
using VaingloryMoba.Characters;

namespace VaingloryMoba.UI
{
    public class HealthBar : MonoBehaviour
    {
        private CharacterStats stats;
        private Transform cameraTransform;
        private GameObject backgroundBar;
        private GameObject foregroundBar;
        private Transform foregroundTransform;

        private float barWidth = 1.2f;
        private float barHeight = 0.15f;

        public void Initialize(CharacterStats characterStats, Color teamColor)
        {
            stats = characterStats;
            cameraTransform = Camera.main?.transform;

            // Create background (dark)
            backgroundBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backgroundBar.name = "HealthBar_BG";
            backgroundBar.transform.SetParent(transform);
            backgroundBar.transform.localPosition = Vector3.zero;
            backgroundBar.transform.localScale = new Vector3(barWidth, barHeight, 0.05f);

            var bgCollider = backgroundBar.GetComponent<Collider>();
            if (bgCollider != null) Destroy(bgCollider);

            var bgRenderer = backgroundBar.GetComponent<Renderer>();
            bgRenderer.material = new Material(Shader.Find("Sprites/Default"));
            bgRenderer.material.color = new Color(0.2f, 0.2f, 0.2f);

            // Create foreground (health)
            foregroundBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foregroundBar.name = "HealthBar_FG";
            foregroundBar.transform.SetParent(transform);
            foregroundBar.transform.localPosition = new Vector3(0, 0, -0.03f);
            foregroundBar.transform.localScale = new Vector3(barWidth, barHeight * 0.8f, 0.05f);
            foregroundTransform = foregroundBar.transform;

            var fgCollider = foregroundBar.GetComponent<Collider>();
            if (fgCollider != null) Destroy(fgCollider);

            var fgRenderer = foregroundBar.GetComponent<Renderer>();
            fgRenderer.material = new Material(Shader.Find("Sprites/Default"));
            fgRenderer.material.color = teamColor;
        }

        private void LateUpdate()
        {
            if (stats == null) return;

            // Update health bar fill
            float healthPercent = stats.CurrentHealth / stats.MaxHealth;
            foregroundTransform.localScale = new Vector3(barWidth * healthPercent, barHeight * 0.8f, 0.05f);

            // Offset to keep bar aligned left
            float offset = (barWidth - barWidth * healthPercent) / 2f;
            foregroundTransform.localPosition = new Vector3(-offset, 0, -0.03f);

            // Face camera
            if (cameraTransform != null)
            {
                transform.LookAt(transform.position + cameraTransform.forward);
            }
        }
    }
}
