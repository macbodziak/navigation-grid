using Unity.Mathematics;

namespace Navigation
{
    public struct AStarSearchNodeDataAsync
    {
        public bool walkable;
        public int2 gridPosition;
        public int costSoFar;
        public int cameFrom;
    }
}