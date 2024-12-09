# Navigation Grid  

**Created by:** Maciej Bodziak  
**Email:** [maciej.bodziak@gmail.com](mailto:maciej.bodziak@gmail.com)  

---

## Dependencies and Setup  
The following packages need to be installed:  
1. **Collections**  
2. **Burst**  
3. **Mathematics**  

Additional requirements:  
- The example scene requires **URP**.  
- Enable unsafe code:  
   - Go to **Edit > Project Settings > Player**.  
   - Under **Other Settings**, check **"Allow 'unsafe' code"**.  

---

## Example Scene  
- The folder `Navigation/Examples` contains:  
   - An example scene.  
   - Example scripts demonstrating how to use this package.  

### Using the Example Scene  
1. Open the example scene.  
2. Click the **"test"** GameObject (it has example scripts attached).  
3. To switch between examples:  
   - Enable the chosen example.  
   - Disable all other examples.
   - Only one test script should be enabled at the same time.  
4. Ensure the layers are set correctly for the grid, actors, and obstacles.  

---

## Quick Guide: How to Set Up the Grid  
1. **Create an Empty GameObject**  
   - Reset its transform to zero (recommended).  

2. **Add a Grid Component**  
   - Attach the **HexGrid** or **SquareGrid** component.  

3. **Optional: Bake the Grid**  
   - Click **"Bake Grid"** to generate an empty grid.  

4. **Set the Layer for the NavGrid**  
   - Example: Set the layer to `Grid`.  
   - This is needed to identify mouse clicks, etc.  

5. **Add Obstacles**  
   - Place obstacles in the scene.  
   - Set their layers (e.g., `Obstacle`).  

6. **Create an Actor**  
   - Create an empty GameObject.  
   - Add the **Actor Component** to it.  
   - Add other components as needed, such as colliders etc.  

7. **Place the Actor**  
   - Position the actor roughly where you want it on the grid.  

8. **Configure Grid Generation**  
   - In the **Grid Inspector**, under the "Grid Generation" section:  
      - Set the **Not Walkable Layers** (same as obstacles).  
      - Set the **Grid Collision Layer** (same as the NavGrid GameObject).  

9. **Bake the Grid**  
   - Click **"Bake Grid"**.  
   - This will:  
     - Mark nodes as not walkable based on obstacles.  
     - Adjust actors to align with their assigned grid nodes.  

---

## Pathfinding Methods  
There are three types of methods for finding paths and walkable areas:  

### 1. Standard / Synchronous  
- **Description:**  
   - These methods execute on the **main thread**.  
   - They block execution until they return a result (path/walkable area) or `null` if none is found.  
- **Best Use Case:**  
   - Small maps or short paths where performance overhead is minimal.  
- **Performance:**  
   - Proven to perform well as the default method for simple cases due to no additional overhead.  
- **Example Method:**

   ``static public Path FindPath(NavGrid navGrid, int startIndex, int goalIndex, bool excludeGoal = false)``

### 2. Unity Jobs  
- **Description:**  
   - These methods provide superior performance on large maps, long distances, or wide walkable areas.  
   - However, they have setup and completion overhead.  
- **Best Use Case:**  
   - Large maps or complex pathfinding scenarios.  
- **Example Method:**

   ``static public PathRequest SchedulePath(HexGrid grid, int startIndex, int goalIndex, bool excludeGoal = false)``
- **Performance:**  
   - Further optimization may be needed. Test and profile for your specific needs.  

### 3. Asynchronous Methods  
- **Description:**  
   - These methods allow pathfinding to run asynchronously without blocking the main thread.
   - They do not use Unity Jobs or Burst Compilation, instead they just take the synchrounous implementation and run it on a background thread.
   - easily integrate into c# async workflow.  


### Recommendation  
- Profile your game and test execution times with different methods.  
- Adjust method selection based on performance needs.

---

## Additional Notes  
- This navigation system is designed for **turn-based games**.  
- Moving multiple characters at the same time can cause grid map references to be overwritten when multiple actors try to enter the same node simultaneously.  
   - **Real-Time Games:** Refactoring will be needed to ensure a node is unoccupied before entering it.  

- The **Actor class** includes events that trigger when:  
   - Entering or exiting nodes.  
   - Starting or finishing movement.  

- For pathfinding and walkable-area calculations:  
   - **Sync Methods**: Recommended for simple cases (short paths, small movement areas).  
     - They execute faster than async methods in many cases.  
   - **Async Methods**: Use when necessary but may need optimization.  


---
