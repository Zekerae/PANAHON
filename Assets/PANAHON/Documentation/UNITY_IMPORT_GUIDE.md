# P.A.N.A.H.O.N. - Unity Import Guide (2.5D Tabletop)

## Prerequisites
- Unity 2021 LTS or Unity 2022+ (any render pipeline)
- An existing Unity project or a new empty project

## Method 1: Direct Folder Import (Recommended)
1. Open your Unity project
2. In your file explorer, navigate to the `PANAHON_UnityProject/Assets/PANAHON/` folder
3. Copy the entire `PANAHON/` folder
4. Paste it into your Unity project's `Assets/` directory
5. Wait for Unity to import and compile all scripts
6. Go to **Tools > P.A.N.A.H.O.N. > Generate 2.5D Tabletop Scene**
7. Press **Play** to test!

## Method 2: Unity Package (if packaged)
1. In Unity, go to **Assets > Import Package > Custom Package**
2. Select the `.unitypackage` file
3. Click **Import All**
4. Follow step 6-7 from Method 1

## After Import

### Generating the Scene
- Navigate to **Tools > P.A.N.A.H.O.N. > Generate 2.5D Tabletop Scene**
- This creates the full game board with 4 era tracks, all UI panels, and player tokens
- Alternatively, use **Window > P.A.N.A.H.O.N. > Setup & Control Panel** for a guided setup window

### Creating a GameConfig Asset (Optional)
- In the Setup Control Panel, click **Create GameConfig Asset**
- This creates a ScriptableObject at `Assets/PANAHON/Resources/PanahonGameConfig.asset`
- Use it to tune all game balance parameters without modifying code

### 2.5D Board Layout
The game uses a 2.5D orthographic camera perspective:
- **Camera**: Orthographic, tilted at 58° looking down
- **Board Tiles**: Flat Quads rotated 90° on X (face camera)
- **Player Tokens**: SpriteRenderer-based circular tokens with sorting order
- **Path Lines**: LineRenderers connecting tiles in each era track
- **Era Labels**: TextMesh markers identifying each quadrant

### Assembly Definitions
The project uses two `.asmdef` files:
- `Panahon.Scripts.asmdef` - Runtime scripts
- `Panahon.Editor.asmdef` - Editor-only scripts (won't compile into builds)

## Troubleshooting

### Scripts not compiling?
- Ensure the `PANAHON/` folder is inside `Assets/`
- Check that `.asmdef` files are present in `Scripts/` and `Editor/` folders
- Restart Unity if needed

### Board looks wrong?
- Make sure the main camera is set to **Orthographic** mode
- Use **Tools > P.A.N.A.H.O.N. > Setup Control Panel > Focus Main Camera on Game Board**

### No Tools menu?
- The menu appears after Unity compiles all editor scripts
- Check **Console** for any compilation errors
- The menu is at **Tools > P.A.N.A.H.O.N.** (with periods)
