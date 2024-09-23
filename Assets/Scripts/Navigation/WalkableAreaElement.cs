using Unity.Mathematics;
using UnityEngine;

namespace Navigation
{
    public class WalkableAreaElement
    {
        int m_gridIndex;
        Vector2Int m_gridPosition;
        Vector3 m_worldPosition;
        int m_cost;
        int m_originIndex;

        public int gridIndex { get => m_gridIndex; set => m_gridIndex = value; }
        public Vector2Int gridPosition { get => m_gridPosition; private set => m_gridPosition = value; }
        public Vector3 worldPosition { get => m_worldPosition; private set => m_worldPosition = value; }
        public int cost { get => m_cost; private set => m_cost = value; }
        public int originIndex { get => m_originIndex; private set => m_originIndex = value; }

        public WalkableAreaElement(int gridIndex, Vector2Int gridPosition, Vector3 worldPosition, int cost, int originIndex)
        {
            this.gridIndex = gridIndex;
            this.gridPosition = gridPosition;
            this.worldPosition = worldPosition;
            this.cost = cost;
            this.originIndex = originIndex;
        }
    }
}
