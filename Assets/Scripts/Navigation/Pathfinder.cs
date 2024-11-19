using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System;


namespace Navigation
{
    public class Pathfinder
    {
        //<summary>
        //Synchronous method for finding a Path on a Square Grid that takes start and goal node indexes as arguments. Return a Path if one is found or null if not
        //</summary>
        static public Path FindPath(SquareGrid grid, int startIndex, int goalIndex, bool excludeGoal = false)
        {
            int currentIndex;

            Utils.PriorityQueue<int, int> frontier = new Utils.PriorityQueue<int, int>();  // <id, priority or heuristic>
            AStarSearchNodeData[] nodeData = new AStarSearchNodeData[grid.Count];

            //initilize nodeData
            for (int i = 0; i < grid.Count; i++)
            {
                nodeData[i].costSoFar = int.MaxValue;
                nodeData[i].cameFrom = -1;              //cameFrom represent the index int nodes array of the predesessor
                nodeData[i].walkable = grid.IsWalkable(i);
            }

            if (excludeGoal)
            {
                nodeData[goalIndex].walkable = true;
            }

            //set the cost so far of the starting position to 0
            nodeData[startIndex].costSoFar = 0;

            frontier.Enqueue(startIndex, 0);

            Vector2Int[] neighbours = grid.Neighbours;
            int DiagonalCost = SquareGrid.DiagonalCost;
            int StraightCost = SquareGrid.StraightCost;
            Vector2Int neighbourGridPosition;
            int neighbourIndex;
            int newCost;
            Vector2Int currentGridPosition;

            while (frontier.Count > 0)
            {
                currentIndex = frontier.Dequeue();

                if (currentIndex == goalIndex)
                {
                    return MakePath(grid, goalIndex, nodeData, excludeGoal);
                }

                currentGridPosition = grid.NodeAt(currentIndex).gridCoordinates;


                for (int i = 0; i < 8; i++)
                {
                    neighbourGridPosition = currentGridPosition + neighbours[i];
                    neighbourIndex = grid.IndexAt(neighbourGridPosition);

                    if (neighbourIndex == -1)
                    {
                        continue;
                    }

                    if (i % 2 == 0) // if straight movement, check only this node
                    {
                        if (nodeData[neighbourIndex].walkable == false)
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + (int)(StraightCost * grid.NodeAt(currentIndex).movementCostModifier);
                    }
                    else //if diagonal movement, check this node and the adjacent nodes
                    {
                        if (nodeData[neighbourIndex].walkable == false ||
                        nodeData[grid.IndexAt(currentGridPosition + neighbours[i - 1])].walkable == false ||
                        nodeData[grid.IndexAt(currentGridPosition + neighbours[i + 1])].walkable == false
                        )
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + (int)(DiagonalCost * grid.NodeAt(currentIndex).movementCostModifier);
                    }

                    if (newCost < nodeData[neighbourIndex].costSoFar)
                    {
                        nodeData[neighbourIndex].costSoFar = newCost;
                        nodeData[neighbourIndex].cameFrom = currentIndex;

                        frontier.Enqueue(neighbourIndex, newCost + ManhattanDistance(grid, neighbourIndex, goalIndex));
                    }
                }
            }
            return null;
        }


        //<summary>
        //Synchronous method for finding a Path on a Square Grid that takes start and goal grid coordinates as arguments. Return a Path if one is found or null if not
        //</summary>
        static public Path FindPath(SquareGrid grid, int start_x, int start_z, int goal_x, int goal_z, bool excludeGoal = false)
        {
            if (grid.CheckIfInBound(start_x, start_z) == false)
            {
                return null;
            }

            if (grid.CheckIfInBound(goal_x, goal_z) == false)
            {
                return null;
            }

            return FindPath(grid, grid.NodeAt(start_x, start_z).id, grid.NodeAt(goal_x, goal_z).id, excludeGoal);
        }


        //<summary>
        //Synchronous method for finding a Path on a Hex Grid that takes start and goal node indexes as arguments. Return a Path if one is found or null if not
        //</summary>
        static public Path FindPath(HexGrid grid, int startIndex, int goalIndex, bool excludeGoal = false)
        {

            Utils.PriorityQueue<int, int> frontier = new Utils.PriorityQueue<int, int>();  // <id, priority or heuristic>
            AStarSearchNodeData[] nodeData = new AStarSearchNodeData[grid.Count];

            int currentIndex;

            //initilize nodeData
            for (int i = 0; i < grid.Count; i++)
            {
                nodeData[i].costSoFar = int.MaxValue;
                nodeData[i].cameFrom = -1;              //cameFrom represent the index int nodes array of the predesessor
                nodeData[i].walkable = grid.IsWalkable(i);
            }

            if (excludeGoal)
            {
                nodeData[goalIndex].walkable = true;
            }

            //set the cost so far of the starting position to 0
            nodeData[startIndex].costSoFar = 0;

            frontier.Enqueue(startIndex, 0);

            Vector2Int[] neighboursEven = grid.NeighboursEven;
            Vector2Int[] neighboursOdd = grid.NeighboursOdd;
            Vector2Int[] neighbours;
            Vector2Int neighbourGridPosition;
            int neighbourIndex;
            int newCost;
            Vector2Int currentGridPosition;
            int movementCost = grid.MovementCost;

            while (frontier.Count > 0)
            {
                currentIndex = frontier.Dequeue();

                if (currentIndex == goalIndex)
                {
                    return MakePath(grid, goalIndex, nodeData, excludeGoal);
                }

                currentGridPosition = grid.NodeAt(currentIndex).gridCoordinates;

                if (currentGridPosition.y % 2 == 0)
                {
                    neighbours = neighboursEven;
                }
                else
                {
                    neighbours = neighboursOdd;
                }

                for (int i = 0; i < 6; i++)
                {
                    neighbourGridPosition = currentGridPosition + neighbours[i];
                    neighbourIndex = grid.IndexAt(neighbourGridPosition);

                    if (neighbourIndex == -1)
                    {
                        continue;
                    }


                    if (nodeData[neighbourIndex].walkable == false)
                    {
                        continue;
                    }

                    newCost = nodeData[currentIndex].costSoFar + (int)(movementCost * grid.NodeAt(currentIndex).movementCostModifier);


                    if (newCost < nodeData[neighbourIndex].costSoFar)
                    {
                        nodeData[neighbourIndex].costSoFar = newCost;
                        nodeData[neighbourIndex].cameFrom = currentIndex;

                        frontier.Enqueue(neighbourIndex, newCost + Distance(grid, neighbourIndex, goalIndex));
                    }
                }
            }
            return null;
        }

        //<summary>
        //Synchronous method for finding a Path on a Hex Grid that takes start and goal grid coordinates as arguments. Return a Path if one is found or null if not
        //</summary>
        static public Path FindPath(HexGrid hexGrid, int start_x, int start_z, int goal_x, int goal_z, bool excludeGoal = false)
        {
            if (hexGrid.CheckIfInBound(start_x, start_z) == false)
            {
                return null;
            }

            if (hexGrid.CheckIfInBound(goal_x, goal_z) == false)
            {
                return null;
            }

            return FindPath(hexGrid, hexGrid.NodeAt(start_x, start_z).id, hexGrid.NodeAt(goal_x, goal_z).id, excludeGoal);
        }


        //<summary>
        //Asynchronous method for finding a Path. Return a PathQuery, that can then be checked if Path is already found
        //</summary>
        static public PathRequest SchedulePath(SquareGrid navGrid, Vector2Int startPosition, Vector2Int goalPosition, bool excludeGoal = false)
        {
            // 
            if (navGrid.CheckIfInBound(startPosition.x, startPosition.y) == false)
            {
                return null;
            }

            if (navGrid.CheckIfInBound(goalPosition.x, goalPosition.y) == false)
            {
                return null;
            }

            PathRequest pathQuery = new PathRequest(navGrid);

            for (int i = 0; i < navGrid.Count; i++)
            {
                pathQuery.nodeData[i] = new AStarSearchNodeDataAsync
                {
                    walkable = navGrid.IsWalkable(i),
                    gridCoordinates = new int2(navGrid.NodeAt(i).gridCoordinates.x, navGrid.NodeAt(i).gridCoordinates.y),
                    costSoFar = int.MaxValue,
                    cameFrom = -1,
                    movementCostModifier = navGrid.MovementCostModifierAt(i)
                };
            }

            FindPathOnSquareGridAStarJob job = new FindPathOnSquareGridAStarJob
            {
                nodeData = pathQuery.nodeData,
                openList = pathQuery.openList,
                startIndex = navGrid.IndexAt(startPosition.x, startPosition.y),
                goalIndex = navGrid.IndexAt(goalPosition.x, goalPosition.y),
                resultCost = pathQuery.totalPathCost,
                resultPath = pathQuery.pathElements,
                navGridWidth = navGrid.Width,
                navGridHeight = navGrid.Height,
                navGridTileSize = navGrid.TileSize,
                navGridPosition = navGrid.Position,
                excludeGoal = excludeGoal
            };

            pathQuery.jobHandle = job.Schedule();

            return pathQuery;
        }


        static public PathRequest SchedulePath(HexGrid grid, Vector2Int startPosition, Vector2Int goalPosition, bool excludeGoal = false)
        {
            if (grid.CheckIfInBound(startPosition.x, startPosition.y) == false)
            {
                return null;
            }

            if (grid.CheckIfInBound(goalPosition.x, goalPosition.y) == false)
            {
                return null;
            }

            return SchedulePath(grid, grid.IndexAt(startPosition), grid.IndexAt(goalPosition), excludeGoal);
        }

        static public PathRequest SchedulePath(HexGrid grid, int startIndex, int goalIndex, bool excludeGoal = false)
        {
            PathRequest pathQuery = new PathRequest(grid);

            for (int i = 0; i < grid.Count; i++)
            {
                pathQuery.nodeData[i] = new AStarSearchNodeDataAsync
                {
                    walkable = grid.IsWalkable(i),
                    gridCoordinates = new int2(grid.NodeAt(i).gridCoordinates.x, grid.NodeAt(i).gridCoordinates.y),
                    costSoFar = int.MaxValue,
                    cameFrom = -1,
                    movementCostModifier = grid.MovementCostModifierAt(i)
                };
            }

            FindPathOnHexGridAStarJob job = new FindPathOnHexGridAStarJob
            {
                nodeData = pathQuery.nodeData,
                openList = pathQuery.openList,
                startIndex = startIndex,
                goalIndex = goalIndex,
                resultCost = pathQuery.totalPathCost,
                resultPath = pathQuery.pathElements,
                navGridWidth = grid.Width,
                navGridHeight = grid.Height,
                navGridTileSize = grid.TileSize,
                navGridPosition = grid.Position,
                excludeGoal = excludeGoal
            };

            pathQuery.jobHandle = job.Schedule();

            return pathQuery;
        }

        static private void DebugPrintPath(SquareGrid navGrid, int goalIndex, AStarSearchNodeData[] nodeData)
        {
            int currentIndex = goalIndex;

            Debug.Log("<color=red>-- PATH FOUND --</color>");
            Debug.Log("Total cost: " + nodeData[goalIndex].costSoFar);
            while (currentIndex != -1)
            {
                Debug.Log(navGrid.NodeAt(currentIndex).gridCoordinates.x + " , " + navGrid.NodeAt(currentIndex).gridCoordinates.y);
                currentIndex = nodeData[currentIndex].cameFrom;
            }
        }


        static private Path MakePath(NavGrid navGrid, int goalIndex, AStarSearchNodeData[] nodeData, bool excludeGoal = false)
        {

            int currentIndex = goalIndex;
            int totalCost = nodeData[goalIndex].costSoFar;

            List<PathElement> pathElements = new List<PathElement>();

            if (excludeGoal)
            {
                currentIndex = nodeData[currentIndex].cameFrom;
                totalCost = nodeData[currentIndex].costSoFar;
            }

            while (currentIndex != -1)
            {
                Vector2Int gridPosition = navGrid.NodeAt(currentIndex).gridCoordinates;
                Vector3 worldPosition = navGrid.WorldPositionAt(gridPosition);
                pathElements.Add(new PathElement(currentIndex, new Vector2Int(gridPosition.x, gridPosition.y), worldPosition));

                currentIndex = nodeData[currentIndex].cameFrom;
            }
            pathElements.Reverse();
            return new Path(pathElements, totalCost);
        }


        static private int ManhattanDistance(SquareGrid squareGrid, int checkedIndex, int goalIndex)
        {
            Vector2Int a = squareGrid.NodeAt(checkedIndex).gridCoordinates;
            Vector2Int b = squareGrid.NodeAt(goalIndex).gridCoordinates;
            return SquareGrid.StraightCost * (Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y));
        }

        static private int Distance(NavGrid navGrid, int checkedIndex, int goalIndex)
        {
            Vector2Int a = navGrid.NodeAt(checkedIndex).gridCoordinates;
            Vector2Int b = navGrid.NodeAt(goalIndex).gridCoordinates;
            return (int)((b - a).magnitude);
        }

        //<summary>
        //Synchronous method for finding Reachable Nodes on a Square Grid, given start grid coordinates and movement budget. Returns a Walkable Area if one is found or null if not
        //</summary>
        static public WalkableArea FindWalkableArea(SquareGrid grid, int start_x, int start_z, int budget)
        {
            if (grid.CheckIfInBound(start_x, start_z) == false)
            {
                return null;
            }

            if (budget <= 0)
            {
                return null;
            }

            List<int> areaIndices = new();
            Utils.PriorityQueue<int, int> frontier = new Utils.PriorityQueue<int, int>();  // <id, priority or heuristic>
            AStarSearchNodeData[] nodeData = new AStarSearchNodeData[grid.Count];

            int currentIndex;

            //initilize nodeData
            for (int i = 0; i < grid.Count; i++)
            {
                nodeData[i].costSoFar = int.MaxValue;
                nodeData[i].cameFrom = -1;              //cameFrom represent the index int nodes array of the predesessor
                nodeData[i].walkable = grid.IsWalkable(i);
            }

            //set the cost so far of the starting position to 0
            nodeData[grid.IndexAt(start_x, start_z)].costSoFar = 0;

            frontier.Enqueue(grid.IndexAt(start_x, start_z), 0);

            Vector2Int neighbourGridPosition;
            Vector2Int[] neighbours = grid.Neighbours;
            int neighbourIndex;
            int newCost;
            Vector2Int currentGridPosition;

            while (frontier.Count > 0)
            {
                currentIndex = frontier.Dequeue();

                currentGridPosition = grid.NodeAt(currentIndex).gridCoordinates;

                for (int i = 0; i < 8; i++)
                {
                    neighbourGridPosition = currentGridPosition + neighbours[i];
                    neighbourIndex = grid.IndexAt(neighbourGridPosition);

                    if (neighbourIndex == -1)
                    {
                        continue;
                    }

                    if (i % 2 == 0) // if straight movement, check only this node
                    {
                        if (nodeData[neighbourIndex].walkable == false)
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + (int)(SquareGrid.StraightCost * grid.NodeAt(currentIndex).movementCostModifier);
                    }
                    else //if diagonal movement, check this node and the adjacent nodes
                    {
                        if (nodeData[neighbourIndex].walkable == false ||
                        nodeData[grid.IndexAt(currentGridPosition + neighbours[i - 1])].walkable == false ||
                        nodeData[grid.IndexAt(currentGridPosition + neighbours[i + 1])].walkable == false
                        )
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + (int)(SquareGrid.DiagonalCost * grid.NodeAt(currentIndex).movementCostModifier);
                    }

                    if (newCost <= budget && newCost < nodeData[neighbourIndex].costSoFar)
                    {
                        int previousCost = nodeData[neighbourIndex].costSoFar;

                        nodeData[neighbourIndex].costSoFar = newCost;
                        nodeData[neighbourIndex].cameFrom = currentIndex;

                        //do not add to froniter if already on froniter / already visited
                        if (previousCost == int.MaxValue)
                        {
                            frontier.Enqueue(neighbourIndex, newCost);
                            areaIndices.Add(neighbourIndex);
                        }
                    }

                }

            }

            List<WalkableAreaElement> walkableAreaElements = new List<WalkableAreaElement>(areaIndices.Count);
            Dictionary<int, int> GridToAreaElementsMap = new();

            for (int i = 0; i < areaIndices.Count; i++)
            {
                int areaIndex = areaIndices[i];
                GridToAreaElementsMap.Add(areaIndex, i);
                Vector2Int gridPosition = grid.NodeAt(areaIndex).gridCoordinates;
                Vector3 worldPosition = grid.WorldPositionAt(gridPosition);
                walkableAreaElements.Add(new WalkableAreaElement(areaIndex, gridPosition, worldPosition, nodeData[areaIndex].costSoFar, nodeData[areaIndex].cameFrom));
            }
            return new WalkableArea(grid, walkableAreaElements, GridToAreaElementsMap);
        }

        //<summary>
        //Synchronous method for finding Reachable Nodes on a Hex Grid, given start grid coordinates and movement budget. Returns a Walkable Area if one is found or null if not
        //</summary>
        static public WalkableArea FindWalkableArea(HexGrid grid, int start_x, int start_z, int budget)
        {
            if (grid.CheckIfInBound(start_x, start_z) == false)
            {
                return null;
            }

            if (budget <= 0)
            {
                return null;
            }

            List<int> areaIndices = new();
            Utils.PriorityQueue<int, int> frontier = new Utils.PriorityQueue<int, int>();  // <id, priority or heuristic>
            AStarSearchNodeData[] nodeData = new AStarSearchNodeData[grid.Count];

            int currentIndex;

            //initilize nodeData
            for (int i = 0; i < grid.Count; i++)
            {
                nodeData[i].costSoFar = int.MaxValue;
                nodeData[i].cameFrom = -1;              //cameFrom represent the index int nodes array of the predesessor
                nodeData[i].walkable = grid.IsWalkable(i);
            }

            //set the cost so far of the starting position to 0
            nodeData[grid.IndexAt(start_x, start_z)].costSoFar = 0;

            frontier.Enqueue(grid.IndexAt(start_x, start_z), 0);

            Vector2Int[] neighboursEven = grid.NeighboursEven;
            Vector2Int[] neighboursOdd = grid.NeighboursOdd;
            Vector2Int[] neighbours;
            Vector2Int neighbourGridPosition;
            int neighbourIndex;
            int newCost;
            Vector2Int currentGridPosition;
            int movementCost = grid.MovementCost;

            while (frontier.Count > 0)
            {
                currentIndex = frontier.Dequeue();

                currentGridPosition = grid.NodeAt(currentIndex).gridCoordinates;

                if (currentGridPosition.y % 2 == 0)
                {
                    neighbours = neighboursEven;
                }
                else
                {
                    neighbours = neighboursOdd;
                }

                for (int i = 0; i < 6; i++)
                {
                    neighbourGridPosition = currentGridPosition + neighbours[i];
                    neighbourIndex = grid.IndexAt(neighbourGridPosition);

                    if (neighbourIndex == -1)
                    {
                        continue;
                    }

                    if (nodeData[neighbourIndex].walkable == false)
                    {
                        continue;
                    }

                    newCost = nodeData[currentIndex].costSoFar + (int)(movementCost * grid.NodeAt(currentIndex).movementCostModifier);


                    if (newCost <= budget && newCost < nodeData[neighbourIndex].costSoFar)
                    {
                        int previousCost = nodeData[neighbourIndex].costSoFar;

                        nodeData[neighbourIndex].costSoFar = newCost;
                        nodeData[neighbourIndex].cameFrom = currentIndex;

                        //do not add to froniter if already on froniter / already visited
                        if (previousCost == int.MaxValue)
                        {
                            frontier.Enqueue(neighbourIndex, newCost);
                            areaIndices.Add(neighbourIndex);
                        }
                    }

                }

            }

            List<WalkableAreaElement> walkableAreaElements = new List<WalkableAreaElement>(areaIndices.Count);
            Dictionary<int, int> GridToAreaElementsMap = new();

            for (int i = 0; i < areaIndices.Count; i++)
            {
                int areaIndex = areaIndices[i];
                GridToAreaElementsMap.Add(areaIndex, i);
                Vector2Int gridPosition = grid.NodeAt(areaIndex).gridCoordinates;
                Vector3 worldPosition = grid.WorldPositionAt(gridPosition);
                walkableAreaElements.Add(new WalkableAreaElement(areaIndex, gridPosition, worldPosition, nodeData[areaIndex].costSoFar, nodeData[areaIndex].cameFrom));
            }
            return new WalkableArea(grid, walkableAreaElements, GridToAreaElementsMap);
        }


        static public List<WalkableAreaElement> ScheduleWalkableArea(SquareGrid grid, int start_x, int start_z, int budget)
        {
            throw new NotImplementedException();
            // return null;
        }

        static public List<WalkableAreaElement> ScheduleWalkableArea(HexGrid grid, int start_x, int start_z, int budget)
        {
            throw new NotImplementedException();
            // return null;
        }
    }
}