# OWL Engine (Object World Logic Engine)

OWL Engine is a lightweight 3D world manipulation tool built with C# and WPF.  
It provides a simple and intuitive environment for creating, selecting, moving, and managing objects inside a 3D grid-based world.  
The project focuses on clarity, modularity, and extendability, making it suitable for prototyping logic-based world systems.

---
## 未実装機能（追加予定）
- light block
- MLT installer
- .obj Push
- Events call
##  Features

###  3D Grid System
- Renders a clean, scalable 3D grid.
- Supports raycasting from mouse position to grid coordinates.
- 
###  Object Interaction
- Create objects on the grid.
- Select objects with mouse picking.
- Move objects across the world.
- Delete objects.
- Visual highlight on selected objects.

### Hierarchy Panel
- Displays all objects in the world.
- Automatically updates when objects are added or removed.

### Camera Controls
- Orbit, pan, and zoom around the world.
- Smooth and intuitive movement.

### Raycasting System
- Grid raycaster for placement.
- Object raycaster for selection.
- Accurate hit detection.

### Modular Architecture
- WorldController manages world logic.
- SelectionManager handles object selection.
- Renderer handles 3D drawing.
- InputHelper abstracts keyboard/mouse input.

---

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/neo557/OWL_Engine.git
Open the solution file:

 ## Code
OWL Engine.slnx
Build and run using Visual Studio 2022 or later.

## Usage
- Left Click: Select object / place object

- Right Click: Cancel selection

- Mouse Drag: Move camera

- Scroll Wheel: Zoom

- Delete Key: Remove selected object
## Roadmap
# Planned Features
- Object rotation and scaling

- Save / Load world data

- Custom object types

- Material and color editing

- Undo / Redo system

- UI improvements

## Changelog

# v1.1.0 – Grid Overhaul & Object Rotation Update

## Grid System Improvements
- Implemented a fully redesigned infinite grid system

- Added fine-grid rendering with dynamic subdivision

- Grid now supports adjustable resolution (gridSize)

- Fixed UV mapping to align grid tiles with world coordinates

- Improved grid visibility and stability in 3D space

- Resolved transparency and backface issues on grid plane

### Object Transform Improvements
- Added Y-axis rotation for all objects

- Established a unified Transform3DGroup structure

 - Scale

 - Rotate

 - Translate

- Ensured rotation persists through highlight/unhighlight

- Improved highlight system to avoid destroying transforms

### New 3D Object Geometry
- TriangleObject upgraded from flat polygon → 3D triangular prism

- RectangleObject upgraded from flat quad → 3D box

- Objects now have proper thickness and render correctly from all angles

- Improved selection accuracy and visual clarity

### Rendering & Architecture Enhancements
- Cleaned up transform handling across renderer

- Improved object creation pipeline

- Ensured consistent behavior across all object types

- Fixed several issues related to object visibility and transform resets
- 
# v1.0.0 – Initial Release
- Implemented 3D grid rendering

- Added object creation, selection, movement, and deletion

- Added hierarchy panel

- Added camera orbit controls

- Implemented grid and object raycasting

- Added highlight system for selected objects

- Established modular architecture (WorldController, SelectionManager, Renderer, etc.)

## License
- MIT License
- Feel free to use, modify, and distribute this project.

## Contributing
Contributions are welcome!
If you have ideas, improvements, or bug fixes, feel free to open an issue or submit a pull request.

### Author
Cro (neo557)  
Creator of OWL Engine
