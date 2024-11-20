using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace Navigation
{
    [BurstCompile]
    public struct FindWalkableAreaOnSquareGridAStarJob : IJob
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
        public NativeList<WalkableAreaElement> walkableAreaElements;
        public NativeList<int> gridToAreaKeys;
        public NativeList<int> gridToAreaValues;
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

                for (int i = 0; i < 8; i++)
                {
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
                        newCost = nodeData[currentIndex].costSoFar + (int)(STRAIGHT_COST * nodeData[currentIndex].movementCostModifier);
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
                        newCost = nodeData[currentIndex].costSoFar + (int)(DIAGONAL_COST * nodeData[currentIndex].movementCostModifier);
                    }

                    if (newCost <= budget && newCost < nodeData[neighbourIndex].costSoFar)
                    {
                        int previousCost = nodeData[neighbourIndex].costSoFar;

                        temp = nodeData[neighbourIndex];
                        temp.costSoFar = newCost;
                        temp.cameFrom = currentIndex;
                        nodeData[neighbourIndex] = temp;
                        if (previousCost == int.MaxValue)
                        {
                            openList.Insert(new OpenListElement(neighbourIndex, newCost + CalculateManhattanDistanceCost(nodeData[neighbourIndex].gridCoordinates, newCost)));
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

        private int CalculateManhattanDistanceCost(int2 a, int2 b)
        {
            int xDistance = math.abs(a.x - b.x);
            int yDistance = math.abs(a.y - b.y);

            return STRAIGHT_COST * (xDistance + yDistance);
        }

        private void BuildWalkableArea()
        {
            Vector3 worldPosition;

            for (int i = 0; i < areaIndices.Length; i++)
            {
                int areaIndex = areaIndices[i];
                gridToAreaKeys.Add(areaIndex);
                gridToAreaValues.Add(i);
                int2 gridPosition = nodeData[areaIndex].gridCoordinates;
                worldPosition = navGridPosition + new Vector3(gridPosition.x * navGridTileSize, 0f, gridPosition.y * navGridTileSize);
                walkableAreaElements.Add(new WalkableAreaElement(areaIndex, new Vector2Int(gridPosition.x, gridPosition.y), worldPosition, nodeData[areaIndex].costSoFar, nodeData[areaIndex].cameFrom));
            }
        }

    }
}