# Match-3 Game Setup Guide

## Step 1: Install DOTween

1. Open Unity Package Manager (Window → Package Manager)
2. Click the **+** button → **Add package from git URL**
3. Enter: `https://github.com/nicloay/DOTween.git`
4. OR download from Asset Store (free): https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676
5. After import, click **DOTween Utility Panel** → **Setup DOTween**

## Step 2: Create Tile Prefab

1. Create empty GameObject, name it **"Tile"**
2. Add **SpriteRenderer** component
3. Set **Sprite** to Unity's built-in circle (Knob) or any sprite
4. Add **TileView.cs** script
5. Set **Sprite Renderer** reference in inspector
6. Save as Prefab in `Assets/Prefabs/Tiles/`

## Step 3: Scene Setup

### Create GameManager Object
1. Create empty GameObject, name it **"GameManager"**
2. Add these components:
   - `GameManager.cs`
   - `BoardController.cs`
   - `InputHandler.cs`
   - `TilePool.cs`
   - `SpecialTileHandler.cs`
   - `BlockerHandler.cs`

### Create Board Object
1. Create empty GameObject as child, name it **"Board"**
2. Add `BoardView.cs` script
3. Create child object **"TileParent"** for organizing tiles

### Wire References
In **GameManager** inspector:
- **Board Controller** → BoardController component
- **Board View** → Board object's BoardView
- **Input Handler** → InputHandler component
- **Tile Pool** → TilePool component

In **TilePool** inspector:
- **Tile Prefab** → Your Tile prefab

In **BoardView** inspector:
- **Tile Pool** → TilePool component
- **Tile Parent** → TileParent transform

## Step 4: Create First Level

1. Right-click in Project → **Create → Match3 → Level Data**
2. Name it **"Level_01"**
3. Configure in Inspector:
   - **Width**: 8
   - **Height**: 8
   - **Available Colors**: Red, Blue, Green, Yellow
   - **Max Moves**: 30
   - Add Goal: **CollectGem → Red → 20**

4. Assign to GameManager's **Current Level** field

## Step 5: Camera Setup

1. Set **Camera** to Orthographic
2. Position at (0, 0, -10)
3. Set **Size** to ~6 (adjust based on board size)
4. Set **Background** color to your preference

## Step 6: Test!

Press Play and swipe to match gems!

---

## Odin Level Editor Features

When you select a LevelData asset, you get:
- **TableMatrix** grid for visual level design
- **Validate Level** button to check for errors
- **Initialize Layout Grid** to create custom starting positions
- Drag-and-drop goal management

## Creating More Levels

1. Duplicate Level_01
2. Adjust board size, colors, goals
3. Add blockers using the Blocker Positions dictionary
4. Enable **Use Custom Layout** for hand-crafted puzzles
