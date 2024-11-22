using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;


namespace Navigation
{
    public class WalkableAreaRequest
    {
        public NativeArray<AStarSearchNodeDataAsync> nodeData;
        public NativeHeap<OpenListElement, OpenListComparer> openList;
        public NativeList<WalkableAreaElement> walkableAreaValues;
        public NativeList<int> walkableAreaKeys;
        public NativeList<int> gridToAreaValues;
        public NativeList<int> areaIndices;
        private int m_startIndex;
        private NavGrid m_navGrid;
        public JobHandle m_jobHandle;
        private bool m_valid;

        public WalkableAreaRequest(NavGrid navGrid, int startIndex)
        {
            openList = new NativeHeap<OpenListElement, OpenListComparer>(Allocator.TempJob, navGrid.Count);
            nodeData = new NativeArray<AStarSearchNodeDataAsync>(navGrid.Count, Allocator.TempJob);
            walkableAreaValues = new NativeList<WalkableAreaElement>(100, Allocator.Persistent);
            walkableAreaKeys = new NativeList<int>(Allocator.Persistent);
            gridToAreaValues = new NativeList<int>(Allocator.Persistent);
            areaIndices = new NativeList<int>(Allocator.TempJob);

            m_navGrid = navGrid;
            m_valid = true;
            m_startIndex = startIndex;
        }

        public bool IsComplete { get => m_jobHandle.IsCompleted; }
        public bool Valid { get => m_valid; private set => m_valid = value; }
        public int StartIndex { get => m_startIndex; private set => m_startIndex = value; }

        public WalkableArea Complete()
        {
            m_jobHandle.Complete();

            Dictionary<int, WalkableAreaElement> walkableAreaElementList = new Dictionary<int, WalkableAreaElement>(walkableAreaValues.Length);

            for (int i = 0; i < walkableAreaKeys.Length; i++)
            {
                walkableAreaElementList.Add(walkableAreaKeys[i], walkableAreaValues[i]);
            }

            WalkableArea area = new WalkableArea(m_navGrid, m_startIndex, walkableAreaElementList);

            openList.Dispose();
            nodeData.Dispose();
            walkableAreaValues.Dispose();
            walkableAreaKeys.Dispose();
            gridToAreaValues.Dispose();
            areaIndices.Dispose();
            m_valid = false;
            return area;
        }
    }
}