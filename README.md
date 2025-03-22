# Leveled
### A simple 2D tile-based level editor for learning level design.
Made by Oliver Beebe

## Features
<details>
  <summary>Tile Editing</summary>

  - Tile tools
    - Brush
    - Eraser
    - Rectangle brush
    - Tile picker
    - Tile selection with move/copy/cut/paste
    - Fill tool
  - Select primary and secondary tile while drawing for left/right click
  - Linking groups
    - Link tiles like doors, keys, etc.
    - Easily change linking groups
    - Toggleable mode for viewing linking groups

</details>

<details>
  <summary>Undo and Redo</summary>

  - Revert changes
  - Revert reversions
  - View the active level changelog
</details>

<details>
  <summary>Multiple Tile Types</summary>

  - Ground (grassy dirt, stone, etc.)
  - Hazard (Water)
  - Player
  - Checkpoints
  - Linkable teleporters
  - Bounce pad
  - Collectables
</details>

<details>
  <summary>Gameplay Features</summary>

  - Basic player with run, jump and walljump
  - Toggleable player position tracing
</details>

<details>
  <summary>File Saving</summary>

  - Choose a folder to read/write levels from
  - Levels are saved to JSON
  - Easily swap between levels
  - Easily create new levels
  - Autosave options (periodic, every change, or off)
</details>

<details>
  <summary>Quality of Life</summary>

  - Camera mover tool (or drag with middle click)
  - Center camera on level
  - Tooltips for all editor buttons and tiles (can be toggled on/off)
  - Clearly labeled keyboards shortcuts for most editor functionality
  - UI scaling
  - Confirmation when closing or deleting level files
  - Instantly enter and exit play mode with Spacebar and Escape keys
</details>

## Planned Features

- Configurable tile availability, allowing administrators to enable or disable the use of certain tiles and gameplay elements with an easily shareable config file.
- Configurable player and camera movement, allowing users to adjust the parameters for player and camera movement, as well as providing some good presets.
- More tile types
  - Slopes
  - Simple enemies
  - Locked doors (with keys and toggles)
  - Level end goal
- Moving platforms
- Color picker for linking groups
- Option to load levels when one level is completed.
- Remapping for keyboard shortcuts
