# Vainglory MOBA: Jungle Bush Asset Workflow

This guide details the workflow for generating, coloring, and placing the 3 specific bush types required to recreate the Halcyon Fold jungle art style.

## 1. The Asset List (What to Generate)

We need 3 distinct bush shapes to avoid a repetitive look.

| Asset Name | Shape Description | Usage Location |
| :--- | :--- | :--- |
| **Bush_Standard** | Round, clumpy, dense. Roughly 1x1 aspect ratio. | Tri-Bushes, Lane Ganks. |
| **Bush_Long** | Elongated, hedge-like, slightly curved. Roughly 2x1 aspect ratio. | Shop "Mustache" bushes. |
| **Bush_Corner** | L-Shaped or a large thicket. | Minion Mines. |

---

## 2. AI Generation Prompts (Meshy.ai)

Use **Meshy 6 Preview**. Turn **Symmetry OFF**.

### A. Standard Bush (The "Clump")
*   **Prompt:** "Stylized low poly MOBA bush, dense foliage, hand-painted texture style, fantasy game asset, gradient green to yellowish-brown leaves, organic clumpy shape, soft ambient occlusion shading, slight cel-shaded look, single mesh."
*   **Negative Prompt:** "Realistic, high poly, noisy, blurry, flowers, berries."

### B. Long Bush (The "Hedge")
*   **Prompt:** "Stylized low poly MOBA bush, **elongated wide hedge**, slightly curved shape, dense foliage, hand-painted texture, fantasy style, gradient green to yellow, game asset."

### C. Corner Bush (The "Thicket")
*   **Prompt:** "Stylized low poly MOBA bush, **L-shaped corner thicket**, large dense foliage, hand-painted texture, fantasy style, gradient green to brown, game asset."

---

## 3. Coloring & Materials (Unity Setup)

AI textures can sometimes look washed out or too uniform. To get the **Vainglory Look** (vibrant top, dark bottom):

1.  **Import:** Drag `.glb` file into `Assets/Prefabs/Map/`.
2.  **Extract Materials:** Click the model -> Inspector -> Materials Tab -> Extract Materials.
3.  **Shader Setup:** Use `Universal Render Pipeline/Lit` or `Simple Lit`.
4.  **The Color Tweak:**
    *   **Base Map:** Assign the AI-generated texture.
    *   **Color Tint:** Set to `Hex #CCCCCC` (Light Grey) to slightly darken it if it's too bright.
    *   **Smoothness:** Set to `0.0` or `0.1`. Bushes should be matte, not shiny.
    *   **Emission (Optional):** If it looks too dark in shadows, add a very faint dark green emission (`#001100`) to simulate subsurface scattering.

**Alternative: Custom Gradient Shader**
If the AI texture is bad, create a new Material with a standard green color (`#2D5A2D`) and use Unity's **Fog** to tint the bottom, or paint vertex colors in Blender.

---

## 4. Placement Guide (Where they go)

Assign these Prefabs to the `MapGenerator` script in the Inspector.

### Slot 1: `Bush Standard`
This prefab will be placed at:
*   **Tri-Bushes:** The triangular ambush spots near the map center (X=75, Z=25).
*   **Lane Gank Bushes:** The strategic hiding spots near the lane walls (X=40, Z=32).

### Slot 2: `Bush Long`
This prefab will be placed at:
*   **Jungle Shop:** The two bushes flanking the shopkeeper at the bottom (X=72 & X=88, Z=4).

### Slot 3: `Bush Corner`
This prefab will be placed at:
*   **Minion Mines:** The large bushes providing cover near the mines (X=48 & X=112, Z=18).

---

## 5. Final Polish Checklist
*   [ ] **Colliders:** Did you add a `BoxCollider` to the prefab? Players need to be able to mouse-over or collide (if intended).
*   [ ] **Scale:** AI models often import tiny. Check `MapGenerator.cs` scaling logic or adjust the Prefab's scale factor in import settings to `100`.
*   [ ] **Grounding:** Ensure the bush bottom sinks slightly into the ground so it doesn't look like it's floating.
