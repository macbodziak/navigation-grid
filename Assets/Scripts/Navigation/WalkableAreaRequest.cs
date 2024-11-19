using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;


namespace Navigation
{
    public class WalkableAreaRequest
    {
        public NativeArray<AStarSearchNodeDataAsync> nodeData;
        public NativeHeap<OpenListElement, OpenListComparer> openList;
        public NativeList<WalkableAreaElement> walkableAreaElements;
        public NativeList<int> gridToAreaKeys;
        public NativeList<int> gridToAreaValues;
        public NativeList<int> areaIndices;
        NavGrid m_navGrid;
        public JobHandle m_jobHandle;
        private bool m_valid;

        public WalkableAreaRequest(NavGrid navGrid)
        {
            openList = new NativeHeap<OpenListElement, OpenListComparer>(Allocator.TempJob, navGrid.Count);
            nodeData = new NativeArray<AStarSearchNodeDataAsync>(navGrid.Count, Allocator.TempJob);
            walkableAreaElements = new NativeList<WalkableAreaElement>(100, Allocator.Persistent);
            gridToAreaKeys = new NativeList<int>(Allocator.Persistent);
            gridToAreaValues = new NativeList<int>(Allocator.Persistent);
            areaIndices = new NativeList<int>(Allocator.TempJob);

            m_navGrid = navGrid;
            m_valid = true;
        }

        public bool IsComplete { get => m_jobHandle.IsCompleted; }
        public bool Valid { get => m_valid; private set => m_valid = value; }

        public WalkableArea Complete()
        {
            m_jobHandle.Complete();

            List<WalkableAreaElement> walkableAreaElementList = new List<WalkableAreaElement>(walkableAreaElements.Length);
            Dictionary<int, int> gridToAreaMap = new Dictionary<int, int>(gridToAreaKeys.Length);

            for (int i = 0; i < walkableAreaElements.Length; i++)
            {
                walkableAreaElementList.Add(walkableAreaElements[i]);
            }

            for (int i = 0; i < gridToAreaKeys.Length; i++)
            {
                gridToAreaMap.Add(gridToAreaKeys[i], gridToAreaValues[i]);
            }

            WalkableArea area = new WalkableArea(m_navGrid, walkableAreaElementList, gridToAreaMap);

            openList.Dispose();
            nodeData.Dispose();
            walkableAreaElements.Dispose();
            gridToAreaKeys.Dispose();
            gridToAreaValues.Dispose();
            areaIndices.Dispose();
            m_valid = false;
            return area;

        }
    }
}