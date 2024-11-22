using Unity.Mathematics;
using UnityEngine;

namespace Navigation
{
    public struct PathElement
    {
        int m_nodeIndex;
        Vector2Int m_gridCoordinates;
        Vector3 m_worldPosition;
        
        public int nodeIndex { get => m_nodeIndex; private set => m_nodeIndex = value; }
        public Vector2Int gridCoordinates { get => m_gridCoordinates; private set => m_gridCoordinates = value; }
        public Vector3 worldPosition { get => m_worldPosition; private set => m_worldPosition = value; }

        public PathElement(int nodeIndex, Vector2Int gridCoordinates, Vector3 worldPosition)
        {
            m_nodeIndex = nodeIndex;
            m_gridCoordinates = gridCoordinates;
            m_worldPosition = worldPosition;
        }
    }
}