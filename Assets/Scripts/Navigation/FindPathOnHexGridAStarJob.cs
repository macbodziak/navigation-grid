using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Navigation;
using UnityEngine;
using Unity.Mathematics;


[BurstCompile]
public struct FindPathOnHexGridAStarJob : IJob
{
    static private readonly int MOVEMENT_COST = 10;
    static private readonly int2[] neighboursEvenRow = {
            new int2(0, 1),
            new int2(1, 0),
            new int2(0, -1),
            new int2(-1, -1),
            new int2(-1, 0),
            new int2(-1, 1),
    };

    static private readonly int2[] neighboursOddRow = {
            new int2(1, 1),
            new int2(1, 0),
            new int2(1, -1),
            new int2(0, -1),
            new int2(-1, 0),
            new int2(0, 1),
    };

    public NativeArray<AStarSearchNodeDataAsync> nodeData;
    public NativeHeap<OpenListElement, OpenListComparer> openList;
    public NativeList<PathElement> resultPath;
    public NativeArray<int> resultCost;
    public int startIndex;
    public int goalIndex;
    public int navGridWidth;
    public int navGridHeight;
    public float navGridTileSize;
    public Vector3 navGridPosition;
    public bool excludeGoal;

    public void Execute()
    {
        int currentIndex;
        int neighbourIndex;
        int newCost;
        int2 currentGridPosition;
        int2 neighbourGridPosition;
        int2 goalPosition = GetPositionAtIndex(goalIndex);
        int2[] direction;

        currentIndex = startIndex;


        AStarSearchNodeDataAsync temp = nodeData[startIndex];
        temp.costSoFar = 0;
        nodeData[startIndex] = temp;

        if (excludeGoal)
        {
            temp = nodeData[goalIndex];
            temp.walkable = true;
            nodeData[goalIndex] = temp;
        }

        openList.Insert(new OpenListElement(startIndex, 0));

        resultCost[0] = -1;

        while (openList.Count > 0)
        {
            currentIndex = openList.Pop().index;

            if (currentIndex == goalIndex)
            {
                BuildPath();
                break;
            }

            currentGridPosition = nodeData[currentIndex].gridCoordinates;

            if (currentGridPosition.y % 2 == 0)
            {
                direction = neighboursEvenRow;
            }
            else
            {
                direction = neighboursOddRow;
            }

            for (int i = 0; i < 6; i++)
            {
                neighbourGridPosition = new int2(currentGridPosition.x + direction[i].x, currentGridPosition.y + direction[i].y);

                //check if neighbourIndex is within bounds
                if (PositionInBounds(neighbourGridPosition) == false)
                {
                    continue;
                }

                neighbourIndex = GetIndexAtPosition(neighbourGridPosition);

                if (nodeData[neighbourIndex].walkable == false)
                {
                    continue;
                }
                newCost = nodeData[currentIndex].costSoFar + (int)(MOVEMENT_COST * nodeData[currentIndex].movementCostModifier);

                if (newCost < nodeData[neighbourIndex].costSoFar)
                {
                    temp = nodeData[neighbourIndex];
                    temp.costSoFar = newCost;
                    temp.cameFrom = currentIndex;
                    nodeData[neighbourIndex] = temp;
                    openList.Insert(new OpenListElement(neighbourIndex, newCost + CalculateDistanceCost(nodeData[neighbourIndex].gridCoordinates, goalPosition)));
                }
            }
        }

    }

    private int CalculateDistanceCost(int2 a, int2 b)
    {
        int xDistance = math.abs(a.x - b.x);
        int yDistance = math.abs(a.y - b.y);
        int remaining = math.abs(xDistance - yDistance);

        return MOVEMENT_COST * math.min(xDistance, yDistance) + MOVEMENT_COST * remaining;
    }

    private int2 GetPositionAtIndex(int index)
    {
        return new int2(index % navGridWidth, index / navGridWidth);
    }

    private int GetIndexAtPosition(int2 position)
    {
        return position.x + position.y * navGridWidth;
    }

    private bool PositionInBounds(int2 position)
    {
        return !(position.x < 0 || position.x >= navGridWidth || position.y < 0 || position.y >= navGridHeight);
    }

    private void BuildPath()
    {
        int currentIndex = goalIndex;
        resultCost[0] = nodeData[goalIndex].costSoFar;
        int2 gridPosition;
        Vector3 worldPosition;

        if (excludeGoal)
        {
            currentIndex = nodeData[currentIndex].cameFrom;
            resultCost[0] = nodeData[currentIndex].costSoFar;
        }

        while (currentIndex != -1)
        {
            gridPosition = nodeData[currentIndex].gridCoordinates;
            worldPosition = GridCooridnatesToWorldPosition(navGridPosition, gridPosition);
            resultPath.Add(new PathElement(currentIndex, new Vector2Int(gridPosition.x, gridPosition.y), worldPosition));
            currentIndex = nodeData[currentIndex].cameFrom;
        }
    }

    private Vector3 GridCooridnatesToWorldPosition(Vector3 navGridPosition, int2 griCoordinates)
    {
        float worldX = navGridPosition.x + griCoordinates.x * navGridTileSize + griCoordinates.y % 2 * navGridTileSize * 0.5f;
        float worldZ = navGridPosition.z + griCoordinates.y * navGridTileSize * 0.8660254f;
        return new Vector3(worldX, navGridPosition.y, worldZ);
    }
}
