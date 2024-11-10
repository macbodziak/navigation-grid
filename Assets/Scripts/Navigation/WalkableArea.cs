using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Navigation
{
    public class WalkableArea : IEnumerable
    {
        SquareGrid m_navGrid;
        Dictionary<int, int> m_gridToAreaMap;
        List<WalkableAreaElement> m_areaElements;

        public WalkableArea(SquareGrid navGrid, List<WalkableAreaElement> areaElements, Dictionary<int, int> gridToAreaIndexMap)
        {
            m_navGrid = navGrid;
            m_areaElements = areaElements;
            m_gridToAreaMap = gridToAreaIndexMap;
        }


        public Path GetPathFromGridPosition(Vector2Int position)
        {
            int elementListIndex = m_gridToAreaMap[m_navGrid.IndexAt(position)];
            return GetPathFromInternalIndex(elementListIndex);

        }

        public Path GetPathFromGridIndex(int index)
        {
            int elementListIndex = m_gridToAreaMap[index];
            return GetPathFromInternalIndex(elementListIndex);
        }

        private Path GetPathFromInternalIndex(int index)
        {
            List<PathElement> pathElements = new();
            int totalCost = m_areaElements[index].cost;

            while (m_areaElements[index].originIndex != -1)
            {
                pathElements.Add(new PathElement(m_areaElements[index].gridIndex, m_areaElements[index].gridPosition, m_areaElements[index].worldPosition));
            }

            return new Path(pathElements, totalCost);
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
