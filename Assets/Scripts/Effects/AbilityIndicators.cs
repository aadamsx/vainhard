using UnityEngine;

namespace VaingloryMoba.Effects
{
    /// <summary>
    /// Visual indicators for ability targeting.
    /// </summary>
    public class AbilityIndicators : MonoBehaviour
    {
        public static AbilityIndicators Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject rangeCirclePrefab;
        [SerializeField] private GameObject skillshotLinePrefab;
        [SerializeField] private GameObject aoeCirclePrefab;

        // Cached indicators
        private GameObject rangeCircle;
        private LineRenderer skillshotLine;
        private GameObject aoeCircle;

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

            CreateIndicators();
        }

        private void CreateIndicators()
        {
            // Range circle (shows ability range)
            rangeCircle = CreateRangeCircle();
            rangeCircle.SetActive(false);

            // Skillshot line
            var skillshotObj = new GameObject("SkillshotLine");
            skillshotObj.transform.SetParent(transform);
            skillshotLine = skillshotObj.AddComponent<LineRenderer>();
            skillshotLine.positionCount = 2;
            skillshotLine.startWidth = 0.5f;
            skillshotLine.endWidth = 0.5f;
            skillshotLine.material = new Material(Shader.Find("Sprites/Default"));
            skillshotLine.startColor = new Color(1f, 0.8f, 0.2f, 0.5f);
            skillshotLine.endColor = new Color(1f, 0.8f, 0.2f, 0.3f);
            skillshotObj.SetActive(false);

            // AoE circle (shows ability effect area)
            aoeCircle = CreateAoECircle();
            aoeCircle.SetActive(false);
        }

        private GameObject CreateRangeCircle()
        {
            var circle = new GameObject("RangeCircle");
            circle.transform.SetParent(transform);

            var lineRenderer = circle.AddComponent<LineRenderer>();
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0.5f, 0.8f, 1f, 0.3f);
            lineRenderer.endColor = new Color(0.5f, 0.8f, 1f, 0.3f);

            // Draw circle
            int segments = 64;
            lineRenderer.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle), 0.1f, Mathf.Sin(angle)));
            }

            return circle;
        }

        private GameObject CreateAoECircle()
        {
            var circle = new GameObject("AoECircle");
            circle.transform.SetParent(transform);

            // Filled circle using cylinder
            var fill = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fill.transform.SetParent(circle.transform);
            fill.transform.localPosition = Vector3.zero;
            fill.transform.localScale = new Vector3(1f, 0.02f, 1f);

            var renderer = fill.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            renderer.material.SetFloat("_Mode", 3);
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.renderQueue = 3000;

            Destroy(fill.GetComponent<Collider>());

            // Edge ring
            var edge = new GameObject("Edge");
            edge.transform.SetParent(circle.transform);
            var lineRenderer = edge.AddComponent<LineRenderer>();
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(1f, 0.5f, 0.5f, 0.8f);
            lineRenderer.endColor = new Color(1f, 0.5f, 0.5f, 0.8f);

            int segments = 64;
            lineRenderer.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, 0.1f, Mathf.Sin(angle) * 0.5f));
            }

            return circle;
        }

        /// <summary>
        /// Show range circle around a position
        /// </summary>
        public void ShowRangeCircle(Vector3 center, float range)
        {
            rangeCircle.SetActive(true);
            rangeCircle.transform.position = center;
            rangeCircle.transform.localScale = Vector3.one * range;
        }

        /// <summary>
        /// Hide range circle
        /// </summary>
        public void HideRangeCircle()
        {
            rangeCircle.SetActive(false);
        }

        /// <summary>
        /// Show skillshot line from start to end
        /// </summary>
        public void ShowSkillshotLine(Vector3 start, Vector3 end, float width = 0.5f)
        {
            skillshotLine.gameObject.SetActive(true);
            skillshotLine.SetPosition(0, start + Vector3.up * 0.1f);
            skillshotLine.SetPosition(1, end + Vector3.up * 0.1f);
            skillshotLine.startWidth = width;
            skillshotLine.endWidth = width;
        }

        /// <summary>
        /// Hide skillshot line
        /// </summary>
        public void HideSkillshotLine()
        {
            skillshotLine.gameObject.SetActive(false);
        }

        /// <summary>
        /// Show AoE circle at position
        /// </summary>
        public void ShowAoECircle(Vector3 position, float radius)
        {
            aoeCircle.SetActive(true);
            aoeCircle.transform.position = position + Vector3.up * 0.05f;
            aoeCircle.transform.localScale = Vector3.one * radius * 2f;
        }

        /// <summary>
        /// Hide AoE circle
        /// </summary>
        public void HideAoECircle()
        {
            aoeCircle.SetActive(false);
        }

        /// <summary>
        /// Hide all indicators
        /// </summary>
        public void HideAll()
        {
            HideRangeCircle();
            HideSkillshotLine();
            HideAoECircle();
        }
    }
}
