using UnityEngine;

namespace Navigation
{
    public class PathElement
    {
        int m_gridIndex;
        Vector2Int m_gridPosition;
        Vector3 m_worldPosition;

        public int gridIndex { get => m_gridIndex; private set => m_gridIndex = value; }
        public Vector2Int gridPosition { get => m_gridPosition; private set => m_gridPosition = value; }
        public Vector3 worldPosition { get => m_worldPosition; private set => m_worldPosition = value; }

        public PathElement(int gridIndex, Vector2Int gridPosition, Vector3 worldPosition)
        {
            this.gridIndex = gridIndex;
            this.gridPosition = gridPosition;
            this.worldPosition = worldPosition;
        }
    }
}