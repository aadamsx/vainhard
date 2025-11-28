# Claude Visual Feedback Workflow

## The Problem
Claude cannot see the Unity game directly. When making visual/layout changes, Claude operates on code and mental models, which can lead to incorrect changes that require user screenshots to diagnose.

## The Solution: SceneDumper

A debug tool that dumps scene state to the Unity console and captures screenshots automatically.

### How to Use

1. **In Unity, press F12** during play mode
2. This triggers:
   - Full scene hierarchy dump to console
   - All renderers with positions, sizes, and colors
   - Map layout summary (which elements are LEFT/CENTER/RIGHT)
   - Screenshot saved to `Screenshots/` folder

### Workflow for Visual Changes

**User requests a visual change:**
1. User: "Move the jungle to the other side"
2. Claude: Makes code changes
3. Claude: "Press F12 to dump scene state"
4. User: Presses F12 in Unity
5. Claude: Reads the log output via `tail -f ~/Library/Logs/Unity/Editor.log | grep "SCENE DUMP\|MAP LAYOUT"`
6. Claude: Reads the screenshot from `Screenshots/` folder
7. Claude: Verifies the change is correct or fixes issues

### What Claude Can See

From the scene dump:
```
=== MAP LAYOUT SUMMARY ===
  Map interpretation (X-axis: left=0, center=40, right=80):
    Jungle: RIGHT side, X=52, size=15x40
    Lane: CENTER side, X=40, size=8x60
    LeftTerrain: LEFT side, X=18, size=36x50
```

This tells Claude exactly where visual elements are positioned.

### Log Monitoring Command

Claude monitors logs with:
```bash
tail -f ~/Library/Logs/Unity/Editor.log | grep -E "(SCENE DUMP|HIERARCHY|VISUAL GEOMETRY|MAP LAYOUT|SCREENSHOT)"
```

### Screenshot Location
Screenshots are saved to: `{project}/Screenshots/scene_YYYYMMDD_HHMMSS.png`

Claude can read these directly with the Read tool.

## Key Principle

**After making visual changes, Claude should request F12 dump and verify before saying "done".**
