using UnityEngine;

namespace VaingloryMoba.Effects
{
    /// <summary>
    /// Collection of visual effect helpers.
    /// </summary>
    public static class VisualEffects
    {
        /// <summary>
        /// Create a tap indicator effect at position
        /// </summary>
        public static GameObject CreateTapIndicator(Vector3 position)
        {
            var indicator = new GameObject("TapIndicator");
            indicator.transform.position = position + Vector3.up * 0.1f;

            // Create expanding ring
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.transform.SetParent(indicator.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);

            var renderer = ring.GetComponent<Renderer>();
            renderer.material.color = new Color(1f, 1f, 1f, 0.5f);

            Object.Destroy(ring.GetComponent<Collider>());

            // Add animation
            var animator = indicator.AddComponent<TapIndicatorAnimator>();
            animator.duration = 0.3f;

            return indicator;
        }

        /// <summary>
        /// Create a damage number popup
        /// </summary>
        public static void CreateDamageNumber(Vector3 position, float damage, bool isCrit = false)
        {
            var popup = new GameObject("DamageNumber");
            popup.transform.position = position + Vector3.up * 2f;

            // Add text mesh
            var textMesh = popup.AddComponent<TextMesh>();
            textMesh.text = Mathf.RoundToInt(damage).ToString();
            textMesh.characterSize = isCrit ? 0.3f : 0.2f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = isCrit ? Color.yellow : Color.white;

            // Billboard
            var billboard = popup.AddComponent<Billboard>();

            // Float up and fade
            var animator = popup.AddComponent<DamageNumberAnimator>();
            animator.duration = 1f;
        }

        /// <summary>
        /// Create a hit spark effect
        /// </summary>
        public static GameObject CreateHitSpark(Vector3 position, Color color)
        {
            var spark = new GameObject("HitSpark");
            spark.transform.position = position;

            // Create multiple small spheres
            for (int i = 0; i < 5; i++)
            {
                var particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.transform.SetParent(spark.transform);
                particle.transform.localPosition = Random.insideUnitSphere * 0.2f;
                particle.transform.localScale = Vector3.one * 0.1f;

                var renderer = particle.GetComponent<Renderer>();
                renderer.material.color = color;

                Object.Destroy(particle.GetComponent<Collider>());
            }

            var animator = spark.AddComponent<HitSparkAnimator>();
            animator.duration = 0.2f;

            return spark;
        }

        /// <summary>
        /// Create a status effect indicator above a unit
        /// </summary>
        public static GameObject CreateStatusIndicator(Transform target, string status, Color color)
        {
            var indicator = new GameObject($"Status_{status}");
            indicator.transform.SetParent(target);
            indicator.transform.localPosition = Vector3.up * 2.5f;

            var textMesh = indicator.AddComponent<TextMesh>();
            textMesh.text = status;
            textMesh.characterSize = 0.15f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = color;

            var billboard = indicator.AddComponent<Billboard>();

            return indicator;
        }
    }

    /// <summary>
    /// Animates tap indicator (expand and fade)
    /// </summary>
    public class TapIndicatorAnimator : MonoBehaviour
    {
        public float duration = 0.3f;
        private float elapsed;
        private Vector3 startScale;

        private void Start()
        {
            startScale = transform.GetChild(0).localScale;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Expand
            float scale = 1f + t * 2f;
            transform.GetChild(0).localScale = new Vector3(startScale.x * scale, startScale.y, startScale.z * scale);

            // Fade
            var renderer = transform.GetChild(0).GetComponent<Renderer>();
            if (renderer != null)
            {
                Color c = renderer.material.color;
                c.a = 1f - t;
                renderer.material.color = c;
            }
        }
    }

    /// <summary>
    /// Animates damage numbers (float up and fade)
    /// </summary>
    public class DamageNumberAnimator : MonoBehaviour
    {
        public float duration = 1f;
        private float elapsed;
        private Vector3 startPos;
        private TextMesh textMesh;

        private void Start()
        {
            startPos = transform.position;
            textMesh = GetComponent<TextMesh>();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Float up
            transform.position = startPos + Vector3.up * t * 1.5f;

            // Fade
            if (textMesh != null)
            {
                Color c = textMesh.color;
                c.a = 1f - t;
                textMesh.color = c;
            }

            // Scale down at end
            if (t > 0.7f)
            {
                float scale = 1f - (t - 0.7f) / 0.3f;
                transform.localScale = Vector3.one * scale;
            }
        }
    }

    /// <summary>
    /// Animates hit sparks (expand and fade)
    /// </summary>
    public class HitSparkAnimator : MonoBehaviour
    {
        public float duration = 0.2f;
        private float elapsed;

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Expand particles outward
            foreach (Transform child in transform)
            {
                child.localPosition += child.localPosition.normalized * Time.deltaTime * 2f;

                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color c = renderer.material.color;
                    c.a = 1f - t;
                    renderer.material.color = c;
                }
            }
        }
    }

    /// <summary>
    /// Makes object always face camera
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }
        }
    }
}
