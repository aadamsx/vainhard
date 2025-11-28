# Root Cause Analysis: Object Positioning Mismatch

## Issue Description
The user reported that objects in the Unity scene are positioned incorrectly relative to the provided design sketch. Specifically, "calculations on where to position objects are just off".

## Investigation Findings
Analysis of the codebase (`MapGenerator.cs`, `JungleManager.cs`, `MinionSpawner.cs`) and the provided sketch (`current_state.png` / description) reveals a fundamental discrepancy in the coordinate system logic.

### 1. Top-Aligned vs. Center-Aligned Lane
*   **Current Code Logic:** The code explicitly calculates the Base position such that the **top** of the base touches the Lane.
    *   `LANE_Z = 54` (Map Top is 60).
    *   `BASE_CENTER_Z = 34`.
    *   `BASE_HEIGHT_Z = 40` (extending from Z=14 to Z=54).
    *   Result: The Lane runs along the absolute northern edge of the playable area. The Bases hang "south" of the lane.
*   **Sketch Design:** The sketch shows the "Straight Lane" connecting the **centers** of the "Base Areas". The Base Area blobs extend significantly *above* and *below* the lane junction.

### 2. Vertical Compression
*   By forcing the lane to the top (Z=54) and the Kraken to the bottom (Z=8), with the Bases spanning Z=14 to Z=54, the "Jungle" area is compressed into a small strip or overlaps ambiguously with the lower parts of the base.
*   The visual weight of the map in the code is top-heavy (inverted U-shape), whereas the sketch suggests a balanced "dumbbell" shape with a jungle area hanging below the central axis.

## Root Cause
The primary error is the definition of `BASE_CENTER_Z` relative to `LANE_Z`.
*   **Faulty Logic:** `private const float BASE_CENTER_Z = 34f; // 54 - 20 = 34`
*   **Correct Logic:** The Lane should be the central axis of the Bases. `BASE_CENTER_Z` should equal `LANE_Z`.

## Proposed Solution
Refactor the coordinate constants in `MapGenerator.cs` (and dependent files `JungleManager.cs`, `MinionSpawner.cs`) to center the layout vertically.

### New Coordinates (Map Size 100x60)
*   **LANE_Z:** Move from `54` to `40`. This leaves 20 units of space above for the top half of the bases.
*   **BASE_CENTER_Z:** Set to `40` (Align with Lane).
    *   Base extends Z=20 to Z=60. (Fits perfectly in map bounds).
*   **KRAKEN_Z:** Keep at `8` or move to `10`.
*   **Jungle Area:** Located between Z=8 (Kraken) and Z=20 (Bottom of Base).

This change will align the Unity scene with the visual topology of the sketch.
