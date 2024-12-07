--- Navigation Grid ---

Created by: Maciej Bodziak, maciej.bodziak@gmail.com

---Dependancies and set up:
-the following packages need to be installed 
1. Collections
2. Burst
3. Mathematics
- The example scene requires URP
- Go to Edit > Project Settings > Player, under Other Settings find and check "Allow 'unsafe' code"

---Example:
- Folder Navigation/Examples contains an example scene as well as example scripts to demonstrate
how to use this package.
- In the example scene, click the "test" game object. It has the example scripts attached. to Switch
between examples, enable the chosen example, and disable all other.

---Quick guide how to set up the grid
1. Create an empty GameObject. It is recommended to reset transform to zero.
2. Add HexGrid or SquareGrid component
3. Optional - click "Bake Grid" to generate an empty grid
4. Set the Layer of the NavGrid gameOject, for example "Grid". This will be needed to identify 
mouse clicks etc
5. Place Obstacles in the scene. Set their Layers, for example "Obstacle"
6. Create an empty gameObject and add the Actor Component to it. Add other component as reqiured, 
such as colliders, modes etc.
7. Place Actor in the scene rougly where you want it to be on the grid
8. In the Grid Inspector, in the Grid Generation section set the appropiate Not Walkable Layers 
(the same as obstacles) and Grid Collision Layer (same as NavGrid gameObject)
9. Click "Bake Grid" to generate the grid. It will account for the obstacles and set the proper 
nodes to not-walkable. It will also set up the Actors and adjust their position so they are placed 
directly on the node they are assigned to.

---Notes:
- This navigation system is meant for turn based games.
- Moving several character at might cause references on the grid map to be overritten, if several 
Actors would try to enter it at once. 
- In order to use it for real time games, refactor will be needed, that checks if a node is 
unoccupied before entering it.
- Actor class has events that invoke when when exiting and entering nodes, as well as movement start
and finish. 
- There are sync and async methods for pathfinding and walkable-area finding. It is recommended to use
sync methods for simple cases (short paths, small movement range in walkable areas), as those methods 
seem to execute fast, faster than async. Further optimalisation will be needed in order to improve async, 
altough it is still good.
- It is also advices to use Task.Run with the sync method as parameter, as this has shown better performance
results. However, make sure to not edit the map until the task completes