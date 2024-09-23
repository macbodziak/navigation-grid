using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Navigation;
using UnityEngine;
using Unity.Mathematics;


[BurstCompile]
public struct FindPathAStarJob : IJob
{
    static private readonly int DIAGONAL_COST = 14;
    static private readonly int STRAIGHT_COST = 10;
    static private readonly int2[] direction = {
        new int2(0,1),  //up
        new int2(1,1),  //up-right
        new int2(1,0), //right
        new int2(1,-1), //down-right
        new int2(0,-1), // down
        new int2(-1,-1), // down-left
        new int2(-1,0), // left
        new int2(-1,1),  //up-left
        new int2(0,1)  //up (again)
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

    public void Execute()
    {
        int currentIndex;
        int neighbourIndex;
        int newCost;
        int2 currentGridPosition;
        int2 neighbourGridPosition;
        int2 goalPosition = GetPositionAtIndex(goalIndex);

        currentIndex = startIndex;

        AStarSearchNodeDataAsync temp = nodeData[startIndex];
        temp.costSoFar = 0;
        nodeData[startIndex] = temp;

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

            for (int i = 0; i < 8; i++)
            {
                currentGridPosition = nodeData[currentIndex].gridPosition;
                neighbourGridPosition = new int2(currentGridPosition.x + direction[i].x, currentGridPosition.y + direction[i].y);

                //check if neighbourIndex is within bounds
                if (PositionInBounds(neighbourGridPosition) == false)
                {
                    continue;
                }
                neighbourIndex = GetIndexAtPosition(neighbourGridPosition);

                //if straight movement check only one node
                if (i % 2 == 0)
                {
                    if (nodeData[neighbourIndex].walkable == false)
                    {
                        continue;
                    }
                    newCost = nodeData[currentIndex].costSoFar + STRAIGHT_COST;
                }
                //If diagonal movement check this node and the adjacent nodes 
                else
                {
                    int2 leftPosition = new int2(currentGridPosition.x + direction[i - 1].x, currentGridPosition.y + direction[i - 1].y);
                    int2 rightPosition = new int2(currentGridPosition.x + direction[i + 1].x, currentGridPosition.y + direction[i + 1].y);
                    int leftIndex = GetIndexAtPosition(leftPosition);
                    int rightIndex = GetIndexAtPosition(rightPosition);

                    if (nodeData[neighbourIndex].walkable == false || nodeData[leftIndex].walkable == false || nodeData[rightIndex].walkable == false)
                    {
                        continue;
                    }
                    newCost = nodeData[currentIndex].costSoFar + DIAGONAL_COST;
                }

                if (newCost < nodeData[neighbourIndex].costSoFar)
                {
                    temp = nodeData[neighbourIndex];
                    temp.costSoFar = newCost;
                    temp.cameFrom = currentIndex;
                    nodeData[neighbourIndex] = temp;
                    openList.Insert(new OpenListElement(neighbourIndex, newCost + CalculateManhattanDistanceCost(nodeData[neighbourIndex].gridPosition, goalPosition)));
                }
            }
        }

    }

    private int CalculateDistanceCost(int2 a, int2 b)
    {
        int xDistance = math.abs(a.x - b.x);
        int yDistance = math.abs(a.y - b.y);
        int remaining = math.abs(xDistance - yDistance);

        return DIAGONAL_COST * math.min(xDistance, yDistance) + STRAIGHT_COST * remaining;
    }

    private int CalculateManhattanDistanceCost(int2 a, int2 b)
    {
        int xDistance = math.abs(a.x - b.x);
        int yDistance = math.abs(a.y - b.y);

        return STRAIGHT_COST * (xDistance + yDistance);
    }

    private int2 GetPositionAtIndex(int index)
    {
        return new int2(index % navGridWidth, index / navGridWidth);
    }

    private int GetIndexAtPosition(int2 position)
    {
        return position.x + position.y * navGridWidth;
    }

    private bool IndexInBounds(int index)
    {
        return index >= 0 && index < nodeData.Length;
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

        while (currentIndex != -1)
        {
            gridPosition = nodeData[currentIndex].gridPosition;
            worldPosition = navGridPosition + new Vector3(gridPosition.x * navGridTileSize, 0f, gridPosition.y * navGridTileSize);
            resultPath.Add(new PathElement(currentIndex, new Vector2Int(gridPosition.x, gridPosition.y), worldPosition));

            currentIndex = nodeData[currentIndex].cameFrom;
        }
    }
}
