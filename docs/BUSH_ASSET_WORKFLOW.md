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

**Key tips for better results:**
- Be VERY specific about dimensions and shape
- Mention "video game asset" and "single solid mesh"
- Describe the exact silhouette you want
- Always use negative prompts

### Standard Bush (Round Clump) - WORKING
```
A round bushy shrub for a fantasy video game, approximately 1 meter tall
and 1 meter wide, dense clustered leaves, stylized hand-painted look like
League of Legends or Vainglory, vibrant green top fading to dark green
at base, cel-shaded style, single solid mesh, game-ready asset, low poly
stylized foliage, no trunk visible, leaves only
```

**Negative:** `realistic, photorealistic, high poly, flowers, berries, fruits, tree, trunk, branches visible, multiple objects, scene, ground, dirt, rocks`

### Long Bush (Hedge Shape)
```
A wide rectangular hedge bush for a fantasy MOBA video game, shaped like
a horizontal rectangle, 2 meters wide and 0.8 meters tall and 0.5 meters
deep, trimmed flat top like a garden hedge, dense green leaves, stylized
hand-painted texture like Vainglory or League of Legends, vibrant green
color, cel-shaded look, single solid mesh, game-ready low poly asset
```

**Negative:** `realistic, photorealistic, round, spherical, tall, tree, trunk, flowers, berries, multiple objects, scene, ground, high poly`

### Corner Bush (L-Shape Thicket)
```
An L-shaped corner bush for a fantasy MOBA video game, shaped like the
letter L when viewed from above, approximately 2 meters on each arm of
the L, 1 meter tall, dense wild foliage, stylized hand-painted texture
like Dota 2 or League of Legends, dark green to brown gradient,
cel-shaded style, single solid mesh, game-ready low poly asset
```

**Negative:** `realistic, photorealistic, round, square, symmetrical, tree, trunk, flowers, high poly, multiple objects, scene`

### Pro Tips for Meshy
1. **If it comes out wrong:** Regenerate 3-4 times, pick the best one
2. **Too round?** Add "rectangular", "flat top", "trimmed" to prompt
3. **Too tall?** Specify exact dimensions: "0.5 meters tall"
4. **Multiple meshes?** Add "single solid mesh, one object only"
5. **Weird colors?** Be specific: "vibrant forest green, RGB 34 139 34"

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
