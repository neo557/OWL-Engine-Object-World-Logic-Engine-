# OWL-Engine-Object-World-Logic-Engine-
An engine application that assists with and manages the placement and setup of 3D objects.

# OWL Engine (Object World Logic Engine)

OWL Engine is a lightweight 3D world manipulation tool built with C# and WPF.  
It provides a simple and intuitive environment for creating, selecting, moving, and managing objects inside a 3D grid-based world.  
The project focuses on clarity, modularity, and extendability, making it suitable for prototyping logic-based world systems.

---

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
