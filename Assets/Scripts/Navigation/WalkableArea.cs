using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Navigation
{
    public class WalkableArea : IEnumerable
    {
        NavGrid m_navGrid;
        Dictionary<int, int> m_gridToAreaMap;
        List<WalkableAreaElement> m_areaElements;

        public WalkableArea(NavGrid navGrid, List<WalkableAreaElement> areaElements, Dictionary<int, int> gridToAreaIndexMap)
        {
            m_navGrid = navGrid;
            m_areaElements = areaElements;
            m_gridToAreaMap = gridToAreaIndexMap;
        }


        public Path GetPathFromGridCoordinates(Vector2Int gridCoordinates)
        {
            int elementListIndex = m_gridToAreaMap[m_navGrid.IndexAt(gridCoordinates)];
            return GetPathFromInternalIndex(elementListIndex);

        }

        public Path GetPathFromNodeIndex(int nodeIndex)
        {
            int elementListIndex = m_gridToAreaMap[nodeIndex];
            return GetPathFromInternalIndex(elementListIndex);
        }

        private Path GetPathFromInternalIndex(int index)
        {
            List<PathElement> pathElements = new();
            int totalCost = m_areaElements[index].cost;

            int nodeIndex = m_areaElements[index].gridIndex;
            Vector2Int gridCoordinates = m_areaElements[index].gridCoordinates;
            Vector3 worldPosition = m_areaElements[index].worldPosition;

            while (m_areaElements[index].originIndex != -1)
            {
                pathElements.Add(new PathElement(nodeIndex, gridCoordinates, worldPosition));

                index = m_areaElements[index].originIndex;

                nodeIndex = m_areaElements[index].gridIndex;
                gridCoordinates = m_areaElements[index].gridCoordinates;
                worldPosition = m_areaElements[index].worldPosition;


            }

            return new Path(pathElements, totalCost);
        }

        public int Count()
        {
            return m_areaElements.Count;
        }

        public bool ContainsNode(int nodeIndex)
        {
            return m_gridToAreaMap.ContainsKey(nodeIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public WalkableAreaEnumator GetEnumerator()
        {
            return new WalkableAreaEnumator(m_areaElements);
        }
    }


    public class WalkableAreaEnumator : IEnumerator
    {
        List<WalkableAreaElement> m_areaElements;
        int position = -1;
        public WalkableAreaEnumator(List<WalkableAreaElement> list)
        {
            m_areaElements = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < m_areaElements.Count);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public WalkableAreaElement Current
        {
            get
            {
                return m_areaElements[position];
            }
        }
    }


}
