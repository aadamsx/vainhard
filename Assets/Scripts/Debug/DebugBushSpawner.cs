using UnityEngine;

namespace VaingloryMoba.DebugTools
{
    public class DebugBushSpawner : MonoBehaviour
    {
        public GameObject bushPrefab;

        void Start()
        {
            if (bushPrefab != null)
            {
                GameObject go = Instantiate(bushPrefab, new Vector3(80, 5, 20), Quaternion.identity); // Center of map, high up
                go.name = "DEBUG_BUSH_TEST";
                
                var filters = go.GetComponentsInChildren<MeshFilter>();
                foreach (var f in filters) {
                    if (f.sharedMesh != null)
                        Debug.Log($"[DebugBush] Found Mesh: {f.name}. Vertices: {f.sharedMesh.vertexCount}. Bounds: {f.sharedMesh.bounds}");
                    else
                        Debug.LogWarning($"[DebugBush] Found Filter on {f.name} but MESH IS NULL!");
                }

                var renderer = go.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    Debug.Log($"[DebugBush] Renderer Bounds: {renderer.bounds}. Pos: {renderer.transform.position}");
                    // renderer.material.color = Color.red; // Comment out to see real material
                }

                else
                {
                    Debug.LogError("[DebugBush] Bush has NO Renderer!");
                }
            }
            else
            {
                Debug.LogError("[DebugBush] No prefab assigned!");
            }
        }
    }
}
