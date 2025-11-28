using UnityEngine;

namespace VaingloryMoba.Utils
{
    /// <summary>
    /// Attaches to a mesh object (like a bush) and paints vertex colors
    /// to create a "Sunlight Gradient" effect (Dark bottom, Light top).
    /// This works with standard Unity URP shaders.
    /// </summary>
    [ExecuteInEditMode]
    public class BushColorizer : MonoBehaviour
    {
        [Header("Gradient Colors")]
        [SerializeField] private Color topColor = new Color(0.6f, 0.8f, 0.2f); // Yellowish Green
        [SerializeField] private Color bottomColor = new Color(0.1f, 0.25f, 0.1f); // Dark Green

        [Header("Settings")]
        [SerializeField] private bool applyOnStart = true;
        [Range(0f, 1f)] [SerializeField] private float gradientOffset = 0.5f; // Shifts the transition point

        private void Start()
        {
            if (applyOnStart)
            {
                PaintBush();
            }
        }

        [ContextMenu("Paint Bush Now")]
        public void PaintBush()
        {
            // Find all mesh filters (root + children)
            MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
            
            if (filters.Length == 0)
            {
                Debug.LogWarning($"[BushColorizer] No MeshFilters found on {name}!");
                return;
            }

            foreach (var mf in filters)
            {
                Mesh mesh = mf.sharedMesh;
                if (mesh == null) continue;

                // Create a copy of the mesh so we don't break the original asset file
                Mesh newMesh = Instantiate(mesh);
                Vector3[] vertices = newMesh.vertices;
                Color[] colors = new Color[vertices.Length];

                // Find height bounds (local to this mesh)
                float minY = float.MaxValue;
                float maxY = float.MinValue;

                foreach (var v in vertices)
                {
                    if (v.y < minY) minY = v.y;
                    if (v.y > maxY) maxY = v.y;
                }

                // Paint vertices
                for (int i = 0; i < vertices.Length; i++)
                {
                    float t = Mathf.InverseLerp(minY, maxY, vertices[i].y);
                    float biasedT = Mathf.Clamp01((t - 0.5f) + gradientOffset);
                    colors[i] = Color.Lerp(bottomColor, topColor, biasedT);
                }

                newMesh.colors = colors;
                mf.mesh = newMesh;
                
                // Ensure the material allows vertex colors
                Renderer r = mf.GetComponent<Renderer>();
                if (r != null && Application.isPlaying)
                {
                    r.material.color = Color.white;
                }
            }
        }
    }
}
