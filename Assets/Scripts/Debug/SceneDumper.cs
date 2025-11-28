using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace VaingloryMoba.Debugging
{
    /// <summary>
    /// Debug tool that dumps scene state to console and captures screenshots.
    /// Press F12 to dump scene + capture screenshot.
    /// This allows Claude to "see" the game state without user screenshots.
    /// </summary>
    public class SceneDumper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode dumpKey = KeyCode.F12;
        [SerializeField] private bool includeInactive = false;
        [SerializeField] private string screenshotPath = "Screenshots";

        private static SceneDumper instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(dumpKey))
            {
                DumpScene();
            }
        }

        /// <summary>
        /// Dumps all scene objects and captures screenshot.
        /// Call via F12 or SceneDumper.Instance.DumpScene()
        /// </summary>
        public void DumpScene()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SCENE DUMP START ===");
            sb.AppendLine($"Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Camera Position: {Camera.main?.transform.position}");
            sb.AppendLine($"Camera Rotation: {Camera.main?.transform.eulerAngles}");
            sb.AppendLine();

            // Group objects by parent/category
            var rootObjects = new List<GameObject>();
            foreach (var go in FindObjectsOfType<GameObject>(includeInactive))
            {
                if (go.transform.parent == null)
                {
                    rootObjects.Add(go);
                }
            }

            // Sort by name for consistency
            rootObjects.Sort((a, b) => a.name.CompareTo(b.name));

            sb.AppendLine("=== HIERARCHY ===");
            foreach (var root in rootObjects)
            {
                DumpGameObject(root, sb, 0);
            }

            sb.AppendLine();
            sb.AppendLine("=== VISUAL GEOMETRY (Renderers) ===");
            DumpRenderers(sb);

            sb.AppendLine();
            sb.AppendLine("=== MAP LAYOUT SUMMARY ===");
            DumpMapLayout(sb);

            sb.AppendLine("=== SCENE DUMP END ===");

            UnityEngine.Debug.Log(sb.ToString());

            // Capture screenshot
            CaptureScreenshot();
        }

        private void DumpGameObject(GameObject go, StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 2);
            string activeMarker = go.activeInHierarchy ? "" : " [INACTIVE]";

            // Get key components
            var components = new List<string>();
            if (go.GetComponent<Renderer>()) components.Add("Renderer");
            if (go.GetComponent<Collider>()) components.Add("Collider");
            if (go.GetComponent<UnityEngine.AI.NavMeshAgent>()) components.Add("NavAgent");

            string componentStr = components.Count > 0 ? $" ({string.Join(", ", components)})" : "";

            sb.AppendLine($"{indent}{go.name}{activeMarker}{componentStr} @ {go.transform.position:F1}");

            // Recurse children (limit depth to avoid spam)
            if (depth < 3)
            {
                foreach (Transform child in go.transform)
                {
                    DumpGameObject(child.gameObject, sb, depth + 1);
                }
            }
            else if (go.transform.childCount > 0)
            {
                sb.AppendLine($"{indent}  ... ({go.transform.childCount} children)");
            }
        }

        private void DumpRenderers(StringBuilder sb)
        {
            var renderers = FindObjectsOfType<Renderer>();
            var geometryList = new List<(string name, Vector3 pos, Vector3 scale, Color color)>();

            foreach (var r in renderers)
            {
                if (!r.gameObject.activeInHierarchy) continue;

                Color color = Color.white;
                if (r.material != null && r.material.HasProperty("_Color"))
                {
                    color = r.material.color;
                }

                geometryList.Add((
                    r.gameObject.name,
                    r.transform.position,
                    r.transform.lossyScale,
                    color
                ));
            }

            // Sort by X then Z position
            geometryList.Sort((a, b) => {
                int xCompare = a.pos.x.CompareTo(b.pos.x);
                return xCompare != 0 ? xCompare : a.pos.z.CompareTo(b.pos.z);
            });

            foreach (var g in geometryList)
            {
                string colorName = GetColorName(g.color);
                sb.AppendLine($"  {g.name}: pos({g.pos.x:F0}, {g.pos.y:F1}, {g.pos.z:F0}) scale({g.scale.x:F1}x{g.scale.y:F1}x{g.scale.z:F1}) {colorName}");
            }
        }

        private void DumpMapLayout(StringBuilder sb)
        {
            // Find key map elements and describe layout
            float mapCenterX = 80f; // Assuming 160 width

            sb.AppendLine("  Map interpretation (X-axis: left=0, center=80, right=160):");

            var renderers = FindObjectsOfType<Renderer>();
            foreach (var r in renderers)
            {
                if (!r.gameObject.activeInHierarchy) continue;

                string name = r.gameObject.name.ToLower();
                Vector3 pos = r.transform.position;
                Vector3 scale = r.transform.lossyScale;

                string side = pos.x < mapCenterX - 5 ? "LEFT" : (pos.x > mapCenterX + 5 ? "RIGHT" : "CENTER");

                if (name.Contains("jungle") || name.Contains("lane") || name.Contains("terrain") ||
                    name.Contains("ground") || name.Contains("boundary") || name.Contains("base"))
                {
                    sb.AppendLine($"    {r.gameObject.name}: {side} side, X={pos.x:F0}, size={scale.x:F0}x{scale.z:F0}");
                }
            }
        }

        private string GetColorName(Color c)
        {
            // Approximate color name for debugging
            if (c.r < 0.3f && c.g > 0.3f && c.b < 0.3f) return "[GREEN]";
            if (c.r < 0.25f && c.g < 0.4f && c.b < 0.25f) return "[DARK GREEN]";
            if (c.r > 0.3f && c.g > 0.4f && c.b < 0.35f) return "[LIGHT GREEN]";
            if (c.r > 0.5f && c.g < 0.4f && c.b < 0.4f) return "[RED]";
            if (c.r < 0.4f && c.g < 0.4f && c.b > 0.5f) return "[BLUE]";
            if (c.r > 0.4f && c.g > 0.4f && c.b < 0.3f) return "[TAN/BROWN]";
            if (c.r < 0.3f && c.g < 0.3f && c.b < 0.3f) return "[DARK]";
            return $"[RGB:{c.r:F1},{c.g:F1},{c.b:F1}]";
        }

        private void CaptureScreenshot()
        {
            // Ensure directory exists
            string fullPath = Path.Combine(Application.dataPath, "..", screenshotPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            string filename = $"scene_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filepath = Path.Combine(fullPath, filename);

            ScreenCapture.CaptureScreenshot(filepath);
            UnityEngine.Debug.Log($"=== SCREENSHOT SAVED: {filepath} ===");
        }

        public static SceneDumper Instance => instance;
    }
}
