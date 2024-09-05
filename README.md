# Game of Life - Måns Fritiofsson
### Instructions
- Open the project and enter play mode
- Enable Gizmos visibility
  - Optionally, disable Canvas gizmo visibility
- Press Left Shift to view controls ingame

### Controls
- Draw cells by holding Left Click
- Kill cells by holding Right Click
- Press Space to pause the simulation
- Scroll up/down to zoom
- Place the mouse at the edges of the screen to move the camera
- Adjust other game settings in the GridManager gameobject

This simulation does not use an array to keep track of the grid, and instead uses a dictionary that keeps track of only the *living* cells and their positions. Nothing will ever happen to dead cells surrounded entirely by dead cells, so by only ticking cells surrounding living cells, performance will be proportional to the amount of living cells, not the total game area. This in turn means the game area can be infinitely large.

Cells are rendered with Debug.DrawLine(), which means you need to enable Gizmo visibility to see them.

![Screenshot_20240905_150856](https://github.com/user-attachments/assets/defd2905-b363-4ab2-a952-87532d9ecf1d)
![Screenshot_20240905_151051](https://github.com/user-attachments/assets/afcd80c3-0977-4b67-87bc-2ef5a153b525)
