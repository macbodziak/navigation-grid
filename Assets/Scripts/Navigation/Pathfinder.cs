using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Navigation
{
    public class Pathfinder
    {

        static private readonly int DIAGONAL_COST = 14;
        static private readonly int STRAIGHT_COST = 10;
        static private readonly Vector2Int[] direction = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
        };

        static public Path FindPath(NavGrid navGrid, int start_x, int start_z, int goal_x, int goal_z)
        {
            if (navGrid.CheckIfInBound(start_x, start_z) == false)
            {
                return null;
            }

            if (navGrid.CheckIfInBound(goal_x, goal_z) == false)
            {
                return null;
            }

            Utils.PriorityQueue<int, int> frontier = new Utils.PriorityQueue<int, int>();  // <id, priority or heuristic>
            AStarSearchNodeData[] nodeData = new AStarSearchNodeData[navGrid.Count];

            int currentIndex;
            int goalIndex = navGrid.IndexAt(goal_x, goal_z);

            //initilize nodeData
            for (int i = 0; i < navGrid.Count; i++)
            {
                nodeData[i].costSoFar = int.MaxValue;
                nodeData[i].cameFrom = -1;              //cameFrom represent the index int nodes array of the predesessor
            }

            //set the cost so far of the starting position to 0
            nodeData[navGrid.IndexAt(start_x, start_z)].costSoFar = 0;

            frontier.Enqueue(navGrid.IndexAt(start_x, start_z), 0);


            while (frontier.Count > 0)
            {

                Vector2Int neighbourGridPosition;
                int neighbourIndex;
                int newCost;
                Vector2Int currentGridPosition;

                currentIndex = frontier.Dequeue();

                if (currentIndex == goalIndex)
                {
                    DebugPrintPath(navGrid, goalIndex, nodeData);
                    return MakePath(navGrid, goalIndex, nodeData);
                }

                currentGridPosition = navGrid.NodeAt(currentIndex).gridPosition;

                for (int i = 0; i < 8; i++)
                {
                    neighbourGridPosition = currentGridPosition + direction[i];
                    neighbourIndex = navGrid.IndexAt(neighbourGridPosition);

                    if (neighbourIndex == -1)
                    {
                        continue;
                    }

                    if (i % 2 == 0) // if straight movement, check only this node
                    {
                        if (navGrid.IsWalkable(neighbourIndex) == false)
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + STRAIGHT_COST;
                    }
                    else //if diagonal movement, check this node and the adjacent nodes
                    {
                        if (navGrid.IsWalkable(neighbourIndex) == false ||
                        navGrid.IsWalkable(navGrid.IndexAt(currentGridPosition + direction[i - 1])) == false ||
                         navGrid.IsWalkable(navGrid.IndexAt(currentGridPosition + direction[i + 1])) == false
                        )
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + DIAGONAL_COST;
                    }

                    if (newCost < nodeData[neighbourIndex].costSoFar)
                    {
                        nodeData[neighbourIndex].costSoFar = newCost;
                        nodeData[neighbourIndex].cameFrom = currentIndex;

                        // frontier.Enqueue(neighbourIndex, newCost);
                        frontier.Enqueue(neighbourIndex, newCost + ManhattanDistance(navGrid, neighbourIndex, goalIndex));
                        // frontier.Enqueue(neighbourIndex, newCost + Distance(navGrid, neighbourIndex, goalIndex));
                    }


                }
            }
            return null;
        }


        static public PathQuery StartPathfinding(NavGrid navGrid, Vector2Int startPosition, Vector2Int goalPosition)
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

            PathQuery pathQuery = new PathQuery(navGrid);

            // int gridSize = navGrid.Count;

            // NativeList<int> openList = new NativeList<int>(navGrid.Count / 2, Allocator.Persistent);
            // NativeArray<AStarSearchNodeDataAsync> nodeData = new NativeArray<AStarSearchNodeDataAsync>(navGrid.Count, Allocator.Persistent);
            // NativeArray<int> totalPathCost = new NativeArray<int>(1, Allocator.Persistent);
            // NativeList<PathElement> resultPath = new NativeList<PathElement>(10, Allocator.Persistent);

            for (int i = 0; i < navGrid.Count; i++)
            {
                pathQuery.nodeData[i] = new AStarSearchNodeDataAsync
                {
                    walkable = navGrid.NodeAt(i).walkable,
                    gridPosition = new int2(navGrid.NodeAt(i).gridPosition.x, navGrid.NodeAt(i).gridPosition.y),
                    costSoFar = int.MaxValue,
                    distanceToGoal = 0,
                    heuristicScore = int.MaxValue,
                    cameFrom = -1,
                    alreadyEvaluated = false
                };
            }

            FindPathAStarJob job = new FindPathAStarJob
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
                navGridPosition = navGrid.Position
            };


            pathQuery.jobHandle = job.Schedule();
            // job.Run();

            return pathQuery;
        }


        static private void DebugPrintPath(NavGrid navGrid, int goalIndex, AStarSearchNodeData[] nodeData)
        {
            int currentIndex = goalIndex;

            Debug.Log("<color=red>-- PATH FOUND --</color>");
            Debug.Log("Total cost: " + nodeData[goalIndex].costSoFar);
            while (currentIndex != -1)
            {
                Debug.Log(navGrid.NodeAt(currentIndex).gridPosition.x + " , " + navGrid.NodeAt(currentIndex).gridPosition.y);
                currentIndex = nodeData[currentIndex].cameFrom;
            }
        }

        static private Path MakePath(NavGrid navGrid, int goalIndex, AStarSearchNodeData[] nodeData)
        {

            int currentIndex = goalIndex;
            int totalCost = nodeData[goalIndex].costSoFar;

            List<PathElement> pathElements = new List<PathElement>();

            while (currentIndex != -1)
            {
                Vector2Int gridPosition = navGrid.NodeAt(currentIndex).gridPosition;
                Vector3 worldPosition = navGrid.GetNodeWorldPosition(gridPosition);
                pathElements.Add(new PathElement(currentIndex, new int2(gridPosition.x, gridPosition.y), worldPosition));

                currentIndex = nodeData[currentIndex].cameFrom;
            }

            return new Path(pathElements, totalCost);
        }

        static private int ManhattanDistance(NavGrid navGrid, int checkedIndex, int goalIndex)
        {
            Vector2Int a = navGrid.NodeAt(checkedIndex).gridPosition;
            Vector2Int b = navGrid.NodeAt(goalIndex).gridPosition;
            return Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y);
        }

        static private int Distance(NavGrid navGrid, int checkedIndex, int goalIndex)
        {
            Vector2Int a = navGrid.NodeAt(checkedIndex).gridPosition;
            Vector2Int b = navGrid.NodeAt(goalIndex).gridPosition;
            return (int)((b - a).magnitude);
        }

        static public List<WalkableAreaElement> FindWalkableArea(NavGrid navGrid, int start_x, int start_z, int budget)
        {
            if (navGrid.CheckIfInBound(start_x, start_z) == false)
            {
                return null;
            }

            if (budget <= 0)
            {
                return null;
            }

            List<int> areaIndices = new();
            List<WalkableAreaElement> walkableAreaElements = new List<WalkableAreaElement>();
            Utils.PriorityQueue<int, int> frontier = new Utils.PriorityQueue<int, int>();  // <id, priority or heuristic>
            AStarSearchNodeData[] nodeData = new AStarSearchNodeData[navGrid.Count];

            int currentIndex;

            //initilize nodeData
            for (int i = 0; i < navGrid.Count; i++)
            {
                nodeData[i].costSoFar = int.MaxValue;
                nodeData[i].cameFrom = -1;              //cameFrom represent the index int nodes array of the predesessor
            }

            //set the cost so far of the starting position to 0
            nodeData[navGrid.IndexAt(start_x, start_z)].costSoFar = 0;

            frontier.Enqueue(navGrid.IndexAt(start_x, start_z), 0);


            while (frontier.Count > 0)
            {

                Vector2Int neighbourGridPosition;
                int neighbourIndex;
                int newCost;
                Vector2Int currentGridPosition;

                currentIndex = frontier.Dequeue();

                // if (currentIndex == goalIndex)
                // {
                //     DebugPrintPath(navGrid, goalIndex, nodeData);
                //     return MakePath(navGrid, goalIndex, nodeData);
                // }

                currentGridPosition = navGrid.NodeAt(currentIndex).gridPosition;

                for (int i = 0; i < 8; i++)
                {
                    neighbourGridPosition = currentGridPosition + direction[i];
                    neighbourIndex = navGrid.IndexAt(neighbourGridPosition);

                    if (neighbourIndex == -1)
                    {
                        continue;
                    }

                    if (i % 2 == 0) // if straight movement, check only this node
                    {
                        if (navGrid.IsWalkable(neighbourIndex) == false)
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + STRAIGHT_COST;
                    }
                    else //if diagonal movement, check this node and the adjacent nodes
                    {
                        if (navGrid.IsWalkable(neighbourIndex) == false ||
                        navGrid.IsWalkable(navGrid.IndexAt(currentGridPosition + direction[i - 1])) == false ||
                         navGrid.IsWalkable(navGrid.IndexAt(currentGridPosition + direction[i + 1])) == false
                        )
                        {
                            continue;
                        }
                        newCost = nodeData[currentIndex].costSoFar + DIAGONAL_COST;
                    }

                    if (newCost <= budget && newCost < nodeData[neighbourIndex].costSoFar)
                    {
                        nodeData[neighbourIndex].costSoFar = newCost;
                        nodeData[neighbourIndex].cameFrom = currentIndex;

                        // frontier.Enqueue(neighbourIndex, newCost);
                        frontier.Enqueue(neighbourIndex, newCost);
                        areaIndices.Add(neighbourIndex);
                        // frontier.Enqueue(neighbourIndex, newCost + Distance(navGrid, neighbourIndex, goalIndex));
                    }


                }





            }
            foreach (int i in areaIndices)
            {
                Vector2Int gridPosition = navGrid.NodeAt(i).gridPosition;
                Vector3 worldPosition = navGrid.GetNodeWorldPosition(gridPosition);
                walkableAreaElements.Add(new WalkableAreaElement(gridPosition, worldPosition, nodeData[i].costSoFar, nodeData[i].cameFrom));
            }
            return walkableAreaElements;
        }
    }
}