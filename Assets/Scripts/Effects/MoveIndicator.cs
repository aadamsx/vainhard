using UnityEngine;

namespace VaingloryMoba.Effects
{
    /// <summary>
    /// Visual indicator showing where the player tapped to move.
    /// </summary>
    public class MoveIndicator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.2f;
        [SerializeField] private float fadeTime = 0.5f;

        private float showTime;
        private float baseScale = 1f;
        private Renderer[] renderers;
        private bool isShowing;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            CreateVisual();
            gameObject.SetActive(false);
        }

        private void CreateVisual()
        {
            // Clear existing children
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            // Create ring indicator
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.transform.SetParent(transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);

            var ringRenderer = ring.GetComponent<Renderer>();
            ringRenderer.material = new Material(Shader.Find("Standard"));
            ringRenderer.material.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            ringRenderer.material.SetFloat("_Mode", 3); // Transparent
            ringRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ringRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ringRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            ringRenderer.material.renderQueue = 3000;

            Destroy(ring.GetComponent<Collider>());

            // Inner dot
            var dot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dot.transform.SetParent(transform);
            dot.transform.localPosition = Vector3.zero;
            dot.transform.localScale = new Vector3(0.2f, 0.03f, 0.2f);

            var dotRenderer = dot.GetComponent<Renderer>();
            dotRenderer.material = new Material(ringRenderer.material);
            dotRenderer.material.color = new Color(0.4f, 1f, 0.4f, 1f);

            Destroy(dot.GetComponent<Collider>());

            // Directional arrows (4 arrows pointing inward)
            for (int i = 0; i < 4; i++)
            {
                var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arrow.transform.SetParent(transform);
                float angle = i * 90f * Mathf.Deg2Rad;
                float distance = 0.5f;
                arrow.transform.localPosition = new Vector3(Mathf.Cos(angle) * distance, 0.02f, Mathf.Sin(angle) * distance);
                arrow.transform.localScale = new Vector3(0.15f, 0.02f, 0.08f);
                arrow.transform.localRotation = Quaternion.Euler(0, -i * 90f, 0);

                var arrowRenderer = arrow.GetComponent<Renderer>();
                arrowRenderer.material = new Material(ringRenderer.material);
                arrowRenderer.material.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);

                Destroy(arrow.GetComponent<Collider>());
            }

            renderers = GetComponentsInChildren<Renderer>();
        }

        private void Update()
        {
            if (!isShowing) return;

            // Rotate
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Pulse scale
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = Vector3.one * baseScale * pulse;

            // Fade out
            float elapsed = Time.time - showTime;
            if (elapsed > fadeTime)
            {
                float fadeProgress = (elapsed - fadeTime) / 0.3f;
                SetAlpha(1f - fadeProgress);

                if (fadeProgress >= 1f)
                {
                    Hide();
                }
            }
        }

        public void Show(Vector3 position)
        {
            transform.position = position + Vector3.up * 0.1f;
            gameObject.SetActive(true);
            isShowing = true;
            showTime = Time.time;
            SetAlpha(1f);
            transform.localScale = Vector3.one * baseScale;
        }

        public void Hide()
        {
            isShowing = false;
            gameObject.SetActive(false);
        }

        private void SetAlpha(float alpha)
        {
            if (renderers == null) return;

            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    Color c = renderer.material.color;
                    c.a = alpha * (c.a > 0.5f ? 1f : 0.8f); // Preserve relative alpha
                    renderer.material.color = c;
                }
            }
        }
    }
}
