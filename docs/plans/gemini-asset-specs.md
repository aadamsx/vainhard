# Asset Specifications for Gemini

This document specifies assets needed for the Vainglory-inspired MOBA prototype. These will be requested from Google's Gemini for generation.

**Note:** Gemini cannot generate 3D model files directly. It can generate:
- 2D textures (PNG)
- UI sprites (PNG)
- Concept art / reference images

For 3D models, use these references with Meshy.ai, Tripo3D, or Unity Asset Store.

---

## Phase 6 Assets (After Gameplay Complete)

### Character: Ringo

**Style Reference:** Vainglory's Ringo - cocky gunslinger with dual pistols

**Concept Art Request:**
```
Generate concept art for a mobile game gunslinger character:
- Male, young adult, confident stance
- Dual revolvers (one in each hand)
- Sleeveless vest, arm wraps
- Loose pants tucked into boots
- Bandolier across chest
- Warm color palette: browns, oranges, gold accents
- Stylized realism (not cartoon, not photorealistic)
- Show front, side, and back views
- Mobile MOBA game style, similar to Vainglory
```

**Texture Request (for 3D model):**
```
Generate character texture atlas (2048x2048) for a gunslinger:
- Skin tone (arms, face)
- Brown leather vest with stitching detail
- Cream/tan arm wraps
- Dark brown pants
- Metallic gold accents for buckles
- Stylized, hand-painted look
- Mobile game optimized (clear details, not noisy)
```

---

### Map: Halcyon Fold

**Ground Textures:**

```
Generate tileable ground texture (1024x1024):
- Fantasy arena stone floor
- Warm grey with subtle cracks
- Slight moss in crevices
- Stylized, not photorealistic
- Mobile game aesthetic
```

```
Generate tileable ground texture (1024x1024):
- Jungle forest floor
- Dark earth with fallen leaves
- Subtle grass patches
- Mystical purple/blue tint
- Stylized, mobile game aesthetic
```

```
Generate tileable ground texture (1024x1024):
- Lane path - worn stone road
- Lighter than surrounding terrain
- Clear travel markings
- Battle-worn appearance
```

**Structure Textures:**

```
Generate texture for defensive turret (1024x1024):
- Ancient stone and metal construction
- Glowing crystal energy core (blue for ally, red for enemy)
- Weathered but powerful appearance
- Fantasy tech aesthetic
```

```
Generate texture for base crystal (1024x1024):
- Large magical crystal
- Inner glow effect
- Blue variant and red variant
- Mystical energy wisps
```

---

### UI Elements

**Ability Icons (256x256 each):**

```
Generate ability icon for "Achilles Shot":
- Single bullet/projectile
- Motion lines suggesting speed
- Orange/gold glow
- Dark background with border
- Mobile game icon style
```

```
Generate ability icon for "Twirling Silver":
- Spinning pistol or bullet cyclone
- Silver metallic sheen
- Speed/motion effect
- Dark background with border
```

```
Generate ability icon for "Hellfire Brew":
- Flaming bottle or fireball
- Red/orange flames
- Intense heat glow
- Dark background with border
```

```
Generate ability icon for "Double Down" (passive):
- Glowing fist or empowered bullet
- Golden aura
- Crit/power indicator
- Dark background with border
```

**HUD Elements:**

```
Generate health bar frame (512x64):
- Fantasy metal frame
- Space for red health fill
- Stylized but readable
- Mobile optimized (clear at small size)
```

```
Generate energy/mana bar frame (512x48):
- Matching style to health bar
- Space for blue energy fill
- Slightly smaller than health bar
```

```
Generate ability button frame (256x256):
- Circular fantasy frame
- Metallic with gem accents
- Space for ability icon inside
- Cooldown overlay compatible
```

---

### Effects (Sprite Sheets)

```
Generate sprite sheet for bullet impact (512x512, 4x4 grid):
- Orange/gold spark explosion
- 16 frames of animation
- Transparent background
- Stylized, not realistic
```

```
Generate sprite sheet for fire effect (512x512, 4x4 grid):
- Red/orange flames
- 16 frames looping animation
- Transparent background
- For Hellfire Brew ability
```

---

## Asset Delivery Format

When requesting from Gemini, specify:
- PNG format
- Specific dimensions listed above
- Transparent background where noted
- sRGB color space
- No watermarks or text

## Integration Notes

1. Save all assets to `Assets/Textures/` or `Assets/UI/`
2. Import settings in Unity:
   - Textures: 2D, Bilinear filter, Compress for mobile
   - UI: Sprite (2D), no compression
   - Sprite sheets: Multiple mode, slice by grid
