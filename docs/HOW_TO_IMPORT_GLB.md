# How to Import a GLB File

## Steps

1. **Drop your GLB file into Unity**
   ```
   Assets/Prefabs/Map/YourModel.glb
   ```

2. **Run the Setup Bushes menu command**
   ```
   Menu: Vainglory > Setup Bushes
   ```

3. **Done.** The script will:
   - Load the GLB
   - Extract the mesh
   - Export to OBJ (Unity-native format)
   - Create a prefab with the mesh
   - Copy to Resources folder

---

## If You Need a New Asset (Not a Bush)

Edit `Assets/Scripts/Editor/SetupBushes.cs` or create a new script:

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public class ImportGLB : EditorWindow
{
    [MenuItem("Vainglory/Import GLB Asset")]
    public static void Import()
    {
        // CHANGE THESE PATHS
        string glbPath = "Assets/YourFolder/YourModel.glb";
        string objPath = "Assets/YourFolder/YourModel.obj";
        string prefabPath = "Assets/YourFolder/YourModel.prefab";

        // 1. Load GLB and extract mesh
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);

        MeshFilter mf = instance.GetComponentInChildren<MeshFilter>();
        Mesh mesh = mf.sharedMesh;
        Material mat = instance.GetComponentInChildren<Renderer>()?.sharedMaterial;

        // 2. Export to OBJ
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Exported from GLB");
        foreach (Vector3 v in mesh.vertices)
            sb.AppendLine($"v {-v.x} {v.y} {v.z}");
        foreach (Vector2 uv in mesh.uv)
            sb.AppendLine($"vt {uv.x} {uv.y}");
        foreach (Vector3 n in mesh.normals)
            sb.AppendLine($"vn {-n.x} {n.y} {n.z}");
        int[] tris = mesh.triangles;
        for (int i = 0; i < tris.Length; i += 3)
        {
            int a = tris[i] + 1;
            int b = tris[i + 2] + 1;
            int c = tris[i + 1] + 1;
            sb.AppendLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
        }
        File.WriteAllText(objPath, sb.ToString());

        // 3. Import OBJ and create prefab
        AssetDatabase.ImportAsset(objPath);
        AssetDatabase.Refresh();

        // 4. Get mesh from OBJ
        Mesh savedMesh = null;
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(objPath))
        {
            if (asset is Mesh m) { savedMesh = m; break; }
        }

        // 5. Create prefab
        GameObject prefab = new GameObject("MyAsset");
        prefab.AddComponent<MeshFilter>().sharedMesh = savedMesh;
        prefab.AddComponent<MeshRenderer>().sharedMaterial = mat;
        prefab.AddComponent<BoxCollider>();

        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

        // 6. Cleanup
        DestroyImmediate(instance);
        DestroyImmediate(prefab);
        AssetDatabase.Refresh();

        Debug.Log($"Created prefab: {prefabPath}");
    }
}
```

---

## Why This Works

Unity doesn't recognize GLB natively, but **UnityGLTF** (already installed) lets you load it via script. The mesh data is there, Unity just won't show it in the Project view.

We export to OBJ because Unity **does** natively import OBJ files. The prefab then references the OBJ mesh, which persists correctly.

---

## Quick Reference

| What | Where |
|------|-------|
| Drop GLB here | `Assets/Prefabs/Map/` |
| Run menu | `Vainglory > Setup Bushes` |
| OBJ output | `Assets/Prefabs/Map/Bush_Exported.obj` |
| Prefab output | `Assets/Prefabs/Map/Bush_Standard_Colored.prefab` |
| Script location | `Assets/Scripts/Editor/SetupBushes.cs` |
