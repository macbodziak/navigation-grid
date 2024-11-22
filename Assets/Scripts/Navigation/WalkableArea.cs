using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace Navigation
{
    public class WalkableArea
    {
        NavGrid m_navGrid;
        Dictionary<int, WalkableAreaElement> m_areaElements;

        public WalkableArea(NavGrid navGrid, Dictionary<int, WalkableAreaElement> areaElements)
        {
            m_navGrid = navGrid;
            m_areaElements = areaElements;
        }


        public Path GetPathFromGridCoordinates(Vector2Int gridCoordinates)
        {
            int nodeIndex = m_navGrid.IndexAt(gridCoordinates);
            return GetPathFromNodeIndex(nodeIndex);

        }

        public Path GetPathFromNodeIndex(int index)
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

        public WalkableAreaElement[] GetWalkableAreaElements()
        {
            return m_areaElements.Values.ToArray();
        }
    }

}
