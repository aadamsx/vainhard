using UnityEngine;
using UnityEditor;
using VaingloryMoba.DebugTools;

namespace VaingloryMoba.Editor
{
    public class AttachDebugSpawner
    {
        [MenuItem("Vainglory/Debug/Spawn Test Bush")]
        public static void SpawnTest()
        {
            GameObject go = new GameObject("DEBUG_SPAWNER");
            var spawner = go.AddComponent<DebugBushSpawner>();
            
            // Load the prefab
            string path = "Assets/Prefabs/Map/Bush_Standard_Colored.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                spawner.bushPrefab = prefab;
                Debug.Log("Created Debug Spawner. Press PLAY to see the bush at (80, 5, 20).");
            }
            else
            {
                Debug.LogError("Could not find the Bush prefab to debug!");
            }
        }
    }
}
