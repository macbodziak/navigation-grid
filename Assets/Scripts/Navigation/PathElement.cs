using Unity.Mathematics;
using UnityEngine;

namespace Navigation
{
    public struct PathElement
    {
        int m_gridIndex;
        Vector2Int m_gridCoordinates;
        Vector3 m_worldPosition;

        public int gridIndex { get => m_gridIndex; private set => m_gridIndex = value; }
        public Vector2Int gridCoordinates { get => m_gridCoordinates; private set => m_gridCoordinates = value; }
        public Vector3 worldPosition { get => m_worldPosition; private set => m_worldPosition = value; }

        public PathElement(int gridIndex, Vector2Int gridCoordinates, Vector3 worldPosition)
        {
            m_gridIndex = gridIndex;
            m_gridCoordinates = gridCoordinates;
            m_worldPosition = worldPosition;
        }
    }
}