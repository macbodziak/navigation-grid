using Unity.Mathematics;

namespace Navigation
{
    public struct AStarSearchNodeDataAsync
    {
        // public int index;
        public bool walkable;
        public int2 gridPosition;
        public int costSoFar;
        public int distanceToGoal;
        public int heuristicScore;
        public int cameFrom;
        public bool alreadyEvaluated;
    }
}