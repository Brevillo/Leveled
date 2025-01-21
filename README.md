# Leveled
### A simple 2D tile-based level editor for learning level design.
Made by Oliver Beebe

## Features
<details>
  <summary>Tile Editing</summary>
  
  - Brush
  - Eraser
  - Rectangle brush
  - Tile picker
  - Tile selection and movement
  - Camera mover
  - Center camera on level
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
</details>

<details>
  <summary>File Saving</summary>

  - Choose a folder to read/write levels from
  - Levels are saved to JSON
  - Easily swap between levels
  - Easily create new levels
</details>

## Planned Features

- Configurable tile availability, allowing administrators to enable or disable the use of certain tiles and gameplay elements with an easily shareable config file.
- Configurable player and camera movement, allowing users to adjust the parameters for player and camera movement, as well as providing some good presets.
- Confirmation of destructive action (popup asking users to save when they switch to another level or close the app).
- More tile types
  - Slopes
  - Simple enemies
  - Locked doors (with keys and toggles)
  - Collectables
  - Level end goal
- Moving platforms
- Secondary palette selection (currently right click is always set to the empty tile)
