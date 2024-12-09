
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Navigation
{
    public class WalkableArea
    {
        NavGrid m_navGrid;
        int m_startIndex;
        Dictionary<int, WalkableAreaElement> m_areaElements;

        public int StartIndex { get => m_startIndex; private set => m_startIndex = value; }

        public WalkableArea(NavGrid navGrid, int startIndex, Dictionary<int, WalkableAreaElement> areaElements)
        {
            m_navGrid = navGrid;
            m_startIndex = startIndex;
            m_areaElements = areaElements;
        }


        public Path GetPathFromGridCoordinates(Vector2Int gridCoordinates)
        {
            int nodeIndex = m_navGrid.IndexAt(gridCoordinates);
            return GetPathFromNodeIndex(nodeIndex);

        }

        public Path GetPathFromNodeIndex(int index, bool includeStart = true)
        {
            if (m_areaElements.ContainsKey(index) == false)
            {
                return null;
            }

            int totalCost = m_areaElements[index].cost;
            List<PathElement> pathElements = new();

            while (m_areaElements.ContainsKey(index))
            {
                WalkableAreaElement element = m_areaElements[index];
                pathElements.Add(new PathElement(index, element.gridCoordinates, element.worldPosition));
                index = element.originIndex;
            }

            if (index == m_startIndex && includeStart)
                pathElements.Add(new PathElement(index, m_navGrid.GridCoordinatesAt(index), m_navGrid.WorldPositionAt(index)));

            pathElements.Reverse();
            return new Path(pathElements, totalCost);
        }

        public int Count()
        {
            return m_areaElements.Count;
        }

        public bool ContainsNode(int nodeIndex)
        {
            return m_areaElements.ContainsKey(nodeIndex);
        }

        public WalkableAreaElement WalkableElementAt(int nodeIndex)
        {
            if (m_areaElements.ContainsKey(nodeIndex))
            {
                return m_areaElements[nodeIndex];
            }

            return new WalkableAreaElement(-1, Vector2Int.zero, Vector3.zero, 0, -1);
        }

        public WalkableAreaElement[] GetWalkableAreaElements()
        {
            return m_areaElements.Values.ToArray();
        }

        public bool IsNodeAdjacent(int NodeIndex)
        {
            List<int> adjacentIndexes = m_navGrid.AdjacentNodeIndexes(NodeIndex);

            foreach (int index in adjacentIndexes)
            {
                if (ContainsNode(index)) return true;
            }
            return false;
        }
    }
}
