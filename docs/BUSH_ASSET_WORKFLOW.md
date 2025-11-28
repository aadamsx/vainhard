# Bush Asset Workflow

## Quick Start - Import a New Bush

1. **Drop GLB into:** `Assets/Prefabs/Map/Stylized_low_poly_MOB_1128001515_generate.glb`
2. **Run menu:** `Vainglory > Setup Bushes`
3. **Done.** Play the game.

---

## Current Working Files

| File | What It Is |
|------|------------|
| `Assets/Prefabs/Map/Stylized_low_poly_MOB_1128001515_generate.glb` | Original GLB from Meshy.ai (35MB) |
| `Assets/Prefabs/Map/Bush_Exported.obj` | Auto-converted OBJ (190MB) - Unity reads this |
| `Assets/Prefabs/Map/Bush_Standard_Colored.prefab` | The working prefab |
| `Assets/Prefabs/Map/BushMaterial.mat` | Green material |
| `Assets/Resources/Prefabs/Bush_Standard_Colored.prefab` | Runtime copy for Resources.Load() |
| `Assets/Scripts/Editor/SetupBushes.cs` | The import script |

---

## What SetupBushes Does

1. Loads GLB via UnityGLTF (installed package)
2. Extracts mesh (vertices, UVs, normals, triangles)
3. Writes to OBJ file (Unity native format)
4. Creates prefab with:
   - MeshFilter → references OBJ mesh
   - MeshRenderer → green material
   - BoxCollider → for collision
   - BushColorizer → gradient coloring
5. Copies prefab to Resources/
6. Assigns to MapGenerator
7. Regenerates map

---

## To Replace With a New Bush Model

**Option A: Same filename**
1. Delete the old GLB
2. Rename your new GLB to `Stylized_low_poly_MOB_1128001515_generate.glb`
3. Drop into `Assets/Prefabs/Map/`
4. Run `Vainglory > Setup Bushes`

**Option B: Different filename**
1. Drop new GLB into `Assets/Prefabs/Map/`
2. Edit `Assets/Scripts/Editor/SetupBushes.cs` line ~16:
   ```csharp
   string glbPath = "Assets/Prefabs/Map/YOUR_NEW_FILE.glb";
   ```
3. Run `Vainglory > Setup Bushes`

---

## AI Generation (Meshy.ai)

Use **Meshy 6 Preview**. Turn **Symmetry OFF**.

### Standard Bush Prompt
```
Stylized low poly MOBA bush, dense foliage, hand-painted texture style,
fantasy game asset, gradient green to yellowish-brown leaves, organic
clumpy shape, soft ambient occlusion shading, slight cel-shaded look,
single mesh.
```

**Negative:** `Realistic, high poly, noisy, blurry, flowers, berries`

### Long Bush Prompt
```
Stylized low poly MOBA bush, elongated wide hedge, slightly curved shape,
dense foliage, hand-painted texture, fantasy style, gradient green to yellow,
game asset.
```

### Corner Bush Prompt
```
Stylized low poly MOBA bush, L-shaped corner thicket, large dense foliage,
hand-painted texture, fantasy style, gradient green to brown, game asset.
```

---

## Bush Placement in Map

Defined in `MapGenerator.cs` → `CreateBushAreas()`:

```csharp
// Left jungle (4 bushes)
CreateBush(parent, new Vector3(35, 0.05f, 12), new Vector2(4, 3), "Standard");
CreateBush(parent, new Vector3(45, 0.05f, 8), new Vector2(5, 3), "Long");
CreateBush(parent, new Vector3(35, 0.05f, 28), new Vector2(4, 3), "Standard");
CreateBush(parent, new Vector3(45, 0.05f, 32), new Vector2(5, 3), "Long");

// Right jungle (4 bushes)
CreateBush(parent, new Vector3(125, 0.05f, 12), new Vector2(4, 3), "Standard");
CreateBush(parent, new Vector3(115, 0.05f, 8), new Vector2(5, 3), "Long");
CreateBush(parent, new Vector3(125, 0.05f, 28), new Vector2(4, 3), "Standard");
CreateBush(parent, new Vector3(115, 0.05f, 32), new Vector2(5, 3), "Long");
```

---

## Troubleshooting

**Bushes invisible:**
```bash
# Check if mesh is null in prefab
cat Assets/Prefabs/Map/Bush_Standard_Colored.prefab | grep m_Mesh
```
- `m_Mesh: {fileID: 0}` = broken, run Setup Bushes again
- `m_Mesh: {fileID: 123, guid: abc}` = good

**GLB not loading:**
- Make sure UnityGLTF package is installed (check Package Manager)
- The GLB won't show in Project view - that's normal, the script can still load it

**Wrong scale:**
- Edit `CreateBush()` in MapGenerator.cs
- Or adjust prefab Transform scale

**Wrong color:**
- Edit `Assets/Prefabs/Map/BushMaterial.mat`
- Or edit `BushColorizer.cs` gradient values
