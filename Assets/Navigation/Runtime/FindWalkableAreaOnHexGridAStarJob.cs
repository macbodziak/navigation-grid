using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Navigation;
using UnityEngine;
using Unity.Mathematics;


[BurstCompile]
public struct FindWalkableAreaOnHexGridAStarJob : IJob
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
    public NativeList<WalkableAreaElement> walkableAreaElements;
    public NativeList<int> areaKeys;
    public NativeList<int> areaIndices;
    public int startIndex;
    public int budget;
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

        AStarSearchNodeDataAsync temp = nodeData[startIndex];
        temp.costSoFar = 0;
        nodeData[startIndex] = temp;

        openList.Insert(new OpenListElement(startIndex, 0));

        while (openList.Count > 0)
        {
            currentIndex = openList.Pop().index;

            currentGridPosition = nodeData[currentIndex].gridCoordinates;

            for (int i = 0; i < 6; i++)
            {
                if (currentGridPosition.y % 2 == 0)
                {
                    neighbourGridPosition = new int2(currentGridPosition.x + neighboursEvenRow[i].x, currentGridPosition.y + neighboursEvenRow[i].y);
                }
                else
                {
                    neighbourGridPosition = new int2(currentGridPosition.x + neighboursOddRow[i].x, currentGridPosition.y + neighboursOddRow[i].y);
                }

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

                if (newCost <= budget && newCost < nodeData[neighbourIndex].costSoFar)
                {
                    int previousCost = nodeData[neighbourIndex].costSoFar;

                    temp = nodeData[neighbourIndex];
                    temp.costSoFar = newCost;
                    temp.cameFrom = currentIndex;
                    nodeData[neighbourIndex] = temp;
                    if (previousCost == int.MaxValue)
                    {
                        openList.Insert(new OpenListElement(neighbourIndex, newCost + CalculateDistanceCost(nodeData[neighbourIndex].gridCoordinates, newCost)));
                        areaIndices.Add(neighbourIndex);
                    }
                }
            }
        }

        BuildWalkableArea();

    }

    private bool PositionInBounds(int2 position)
    {
        return !(position.x < 0 || position.x >= navGridWidth || position.y < 0 || position.y >= navGridHeight);
    }

    private int GetIndexAtPosition(int2 position)
    {
        return position.x + position.y * navGridWidth;
    }

    private int CalculateDistanceCost(int2 a, int2 b)
    {
        int xDistance = math.abs(a.x - b.x);
        int yDistance = math.abs(a.y - b.y);
        int remaining = math.abs(xDistance - yDistance);

        return MOVEMENT_COST * math.min(xDistance, yDistance) + MOVEMENT_COST * remaining;
    }

    private void BuildWalkableArea()
    {
        for (int i = 0; i < areaIndices.Length; i++)
        {
            int areaIndex = areaIndices[i];
            areaKeys.Add(areaIndex);
            int2 gridPosition = nodeData[areaIndex].gridCoordinates;
            Vector3 worldPosition = GridCooridnatesToWorldPosition(navGridPosition, gridPosition);
            walkableAreaElements.Add(new WalkableAreaElement(areaIndex, new Vector2Int(gridPosition.x, gridPosition.y), worldPosition, nodeData[areaIndex].costSoFar, nodeData[areaIndex].cameFrom));
        }
    }

    private Vector3 GridCooridnatesToWorldPosition(Vector3 navGridPosition, int2 griCoordinates)
    {
        float worldX = navGridPosition.x + griCoordinates.x * navGridTileSize + griCoordinates.y % 2 * navGridTileSize * 0.5f;
        float worldZ = navGridPosition.z + griCoordinates.y * navGridTileSize * 0.8660254f;
        return new Vector3(worldX, navGridPosition.y, worldZ);
    }
}