# GLB/GLTF Import in Unity: The Definitive Guide

## TL;DR - What Actually Works

**The Problem:** Unity doesn't natively import .glb/.gltf files. Even with UnityGLTF package installed, files may not be recognized.

**The Solution That Worked:**
1. UnityGLTF imports GLB at editor time (not visible in Project view, but loadable via script)
2. Editor script extracts mesh → exports to OBJ format
3. OBJ is natively supported by Unity
4. Prefab references the mesh from the OBJ file

---

## What Went Wrong (The Bullshit)

### Issue 1: GLB File Not Visible in Project View
- File existed on disk: `Assets/Prefabs/Map/Stylized_low_poly_MOB_1128001515_generate.glb` (35MB)
- Unity created a `.meta` file for it
- **BUT** the meta file showed `DefaultImporter` instead of a proper 3D model importer
- This means Unity treated it as an unknown binary file, not a 3D model

**Evidence:**
```yaml
# BAD - What we had:
DefaultImporter:
  externalObjects: {}

# GOOD - What OBJ files get:
ModelImporter:
  serializedVersion: 22200
  ...
```

### Issue 2: Prefab Had Null Mesh Reference
The prefab was created but with `m_Mesh: {fileID: 0}` - meaning no mesh attached.

**Why?** The SetupBushes script tried to:
1. Load GLB as GameObject → `AssetDatabase.LoadAssetAtPath<GameObject>(glbPath)` returned NULL
2. Script silently returned without creating anything useful
3. No error shown because of try/catch swallowing issues

### Issue 3: Going in Circles
Multiple attempts tried to:
- Save mesh as .asset file (didn't work - mesh reference lost)
- Copy prefab to Resources folder (didn't help - still null mesh)
- Various Unity reimport attempts (didn't change the importer type)

---

## The Fix That Actually Worked

### Step 1: Export Mesh to OBJ Format
The SetupBushes script DID successfully:
1. Instantiate the GLB model (via `PrefabUtility.InstantiatePrefab`)
2. Find the MeshFilter component and get the mesh
3. Export vertices, UVs, normals, and triangles to OBJ text format

**The OBJ file was created:** `Assets/Prefabs/Map/Bush_Exported.obj` (190MB)

### Step 2: Unity Natively Imports OBJ
Unity's built-in ModelImporter handles .obj files automatically. The mesh becomes a sub-asset of the OBJ file.

### Step 3: Reference Mesh from OBJ in Prefab
```csharp
// Load all assets from the OBJ file
var allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Prefabs/Map/Bush_Exported.obj");

// Find the Mesh sub-asset
foreach (var asset in allAssets)
{
    if (asset is Mesh mesh)
    {
        // Use this mesh in the prefab
        meshFilter.sharedMesh = mesh;
        break;
    }
}
```

### Step 4: Save Prefab with Proper Reference
```csharp
PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/Prefabs/Map/Bush.prefab");
```

**Result:** Prefab now has:
```yaml
m_Mesh: {fileID: -2432090755550338912, guid: cb9e2b0292d4a41a6ababdc17133a4aa, type: 3}
```
The guid `cb9e2b0292d4a41a6ababdc17133a4aa` points to the OBJ file's mesh.

---

## Clean Workflow for Next Time

### Option A: Use OBJ Instead of GLB (Simplest)
1. Export from Meshy.ai/Blender as .obj instead of .glb
2. Drop into Unity Assets folder
3. Unity imports it automatically
4. Drag into scene or create prefab
5. Done.

### Option B: GLB with Manual Conversion Script
1. Drop GLB into Assets folder
2. Run this editor script:

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public class ConvertGLBToOBJ : EditorWindow
{
    [MenuItem("Tools/Convert GLB to OBJ")]
    public static void Convert()
    {
        // 1. Find the GLB file
        string glbPath = "Assets/Models/YourModel.glb";

        // 2. Load it (UnityGLTF makes this work even though Project view doesn't show it)
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
        if (model == null)
        {
            Debug.LogError("Could not load GLB. Make sure UnityGLTF package is installed.");
            return;
        }

        // 3. Instantiate to access mesh
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        MeshFilter mf = instance.GetComponentInChildren<MeshFilter>();
        Mesh mesh = mf.sharedMesh;

        // 4. Export to OBJ
        string objPath = glbPath.Replace(".glb", ".obj");
        ExportMeshToOBJ(mesh, objPath);

        // 5. Cleanup
        DestroyImmediate(instance);
        AssetDatabase.Refresh();

        Debug.Log($"Converted to: {objPath}");
    }

    static void ExportMeshToOBJ(Mesh mesh, string path)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Exported from Unity");

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
            int b = tris[i + 2] + 1;  // Swap winding
            int c = tris[i + 1] + 1;
            sb.AppendLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
        }

        File.WriteAllText(path, sb.ToString());
    }
}
```

### Option C: Use FBX (Best Native Support)
1. Export from Blender/Meshy as .fbx instead
2. Unity has excellent native FBX support
3. No conversion needed

---

## Debugging Checklist

If your mesh isn't showing:

1. **Check the prefab YAML file:**
   ```bash
   cat Assets/Prefabs/YourPrefab.prefab | grep "m_Mesh"
   ```
   - `m_Mesh: {fileID: 0}` = NULL, broken
   - `m_Mesh: {fileID: 12345, guid: abc123}` = Good, has reference

2. **Check the meta file importer type:**
   ```bash
   cat Assets/Models/YourModel.glb.meta | head -10
   ```
   - `DefaultImporter` = Unity doesn't recognize format
   - `ModelImporter` = Good

3. **Check if mesh exists in imported asset:**
   ```csharp
   var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Models/YourModel.obj");
   foreach (var a in assets) Debug.Log($"{a.name}: {a.GetType()}");
   ```

4. **Check console for silent errors:**
   - Many import issues fail silently
   - Look for any red/yellow messages during import

---

## Key Lessons

1. **Unity's GLB support sucks** - Even with UnityGLTF, the workflow is janky
2. **OBJ/FBX are native** - Just work out of the box
3. **Mesh references in prefabs are GUIDs** - If the source asset changes/moves, references break
4. **Always check the YAML** - Don't trust the Unity inspector, check the actual file
5. **Editor scripts can access GLB meshes** - Even when Project view doesn't show them
6. **Silent failures are the enemy** - Add Debug.Log everywhere when debugging imports

---

## Files in This Project

- `Assets/Prefabs/Map/Stylized_low_poly_MOB_1128001515_generate.glb` - Original from Meshy.ai (unusable directly)
- `Assets/Prefabs/Map/Bush_Exported.obj` - Converted OBJ (190MB, works)
- `Assets/Prefabs/Map/Bush_Standard_Colored.prefab` - Final prefab referencing OBJ mesh
- `Assets/Scripts/Editor/SetupBushes.cs` - The conversion/setup script
