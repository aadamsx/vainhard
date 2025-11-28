using UnityEngine;
using UnityEditor;
using VaingloryMoba.Map;
using VaingloryMoba.Utils;
using System.IO;

namespace VaingloryMoba.Editor
{
    public class SetupBushes : EditorWindow
    {
        [MenuItem("Vainglory/Setup Bushes")]
        public static void Setup()
        {
            try
            {
                string objPath = "Assets/Prefabs/Map/Bush_Exported.obj";
                string prefabPath = "Assets/Prefabs/Map/Bush_Standard_Colored.prefab";

                // The OBJ file already exists from previous export - load mesh directly from it
                Mesh savedMesh = null;
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(objPath);
                Debug.Log($"[SetupBushes] Loading from OBJ: {objPath}, found {allAssets.Length} assets");

                foreach (var asset in allAssets)
                {
                    Debug.Log($"[SetupBushes] Asset: {asset.name} ({asset.GetType().Name})");
                    if (asset is Mesh mesh)
                    {
                        savedMesh = mesh;
                        Debug.Log($"[SetupBushes] Found mesh: {mesh.name}, verts: {mesh.vertexCount}");
                        break;
                    }
                }

                if (savedMesh == null)
                {
                    Debug.LogError("[SetupBushes] No mesh found in OBJ file!");
                    return;
                }

                // BUILD CLEAN PREFAB OBJECT
                GameObject cleanBush = new GameObject("Bush_Standard_Colored");

                // Mesh Filter - reference the mesh from the OBJ
                var mf = cleanBush.AddComponent<MeshFilter>();
                mf.sharedMesh = savedMesh;

                // Mesh Renderer with green material
                var mr = cleanBush.AddComponent<MeshRenderer>();
                Material greenMat = new Material(Shader.Find("Standard"));
                greenMat.color = new Color(0.2f, 0.6f, 0.2f);
                string matPath = "Assets/Prefabs/Map/BushMaterial.mat";
                AssetDatabase.CreateAsset(greenMat, matPath);
                mr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);

                // Collider
                var col = cleanBush.AddComponent<BoxCollider>();
                col.size = savedMesh.bounds.size;
                col.center = savedMesh.bounds.center;

                // Colorizer
                var colorizer = cleanBush.AddComponent<BushColorizer>();

                // Scale - the mesh is about 1.5 units, good size
                Debug.Log($"[SetupBushes] Mesh bounds: {savedMesh.bounds.size}");

                // SAVE AS PREFAB
                PrefabUtility.SaveAsPrefabAsset(cleanBush, prefabPath);
                Debug.Log($"[SetupBushes] Created prefab at {prefabPath}");

                // COPY TO RESOURCES for runtime loading
                string resourcesPath = "Assets/Resources/Prefabs/Bush_Standard_Colored.prefab";
                if (!Directory.Exists("Assets/Resources/Prefabs"))
                {
                    Directory.CreateDirectory("Assets/Resources/Prefabs");
                }
                AssetDatabase.DeleteAsset(resourcesPath); // Remove old one first
                AssetDatabase.CopyAsset(prefabPath, resourcesPath);
                AssetDatabase.Refresh();
                Debug.Log($"[SetupBushes] Copied to Resources: {resourcesPath}");

                // CLEANUP scene object
                DestroyImmediate(cleanBush);

                // ASSIGN to MapGenerator
                MapGenerator mapGen = Object.FindObjectOfType<MapGenerator>();
                if (mapGen != null)
                {
                    SerializedObject so = new SerializedObject(mapGen);
                    GameObject newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    so.FindProperty("bushStandard").objectReferenceValue = newPrefab;
                    so.FindProperty("bushLong").objectReferenceValue = newPrefab;
                    so.FindProperty("bushCorner").objectReferenceValue = newPrefab;
                    so.ApplyModifiedProperties();
                    Debug.Log("[SetupBushes] Assigned prefab to MapGenerator");
                    mapGen.GenerateMap();
                }

                Debug.Log("[SetupBushes] SUCCESS!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SetupBushes] ERROR: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
