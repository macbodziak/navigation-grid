using Unity.Mathematics;
using UnityEngine;

namespace Navigation
{
    public struct PathElement
    {
        int m_gridIndex;
        int2 m_gridPosition;
        Vector3 m_worldPosition;

        public int gridIndex { get => m_gridIndex; private set => m_gridIndex = value; }
        public int2 gridPosition { get => m_gridPosition; private set => m_gridPosition = value; }
        public Vector3 worldPosition { get => m_worldPosition; private set => m_worldPosition = value; }

        public PathElement(int gridIndex, int2 gridPosition, Vector3 worldPosition)
        {
            m_gridIndex = gridIndex;
            m_gridPosition = gridPosition;
            m_worldPosition = worldPosition;
        }
    }
}