using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;


namespace Navigation
{
    public class PathRequest
    {
        public NativeHeap<OpenListElement, OpenListComparer> openList;
        public NativeArray<AStarSearchNodeDataAsync> nodeData;
        public NativeArray<int> totalPathCost;
        public NativeList<PathElement> pathElements;
        public JobHandle jobHandle;
        private bool m_valid;

        public PathRequest(NavGrid navGrid)
        {
            openList = new NativeHeap<OpenListElement, OpenListComparer>(Allocator.TempJob, navGrid.Count);
            nodeData = new NativeArray<AStarSearchNodeDataAsync>(navGrid.Count, Allocator.TempJob);
            totalPathCost = new NativeArray<int>(1, Allocator.Persistent);
            pathElements = new NativeList<PathElement>(100, Allocator.Persistent);
            m_valid = true;
        }

        public bool IsComplete { get => jobHandle.IsCompleted; }
        public bool Valid { get => m_valid; private set => m_valid = value; }

        public Path Complete()
        {
            jobHandle.Complete();
            List<PathElement> pathElementList = new List<PathElement>(pathElements.Length);
            for (int i = 0; i < pathElements.Length; i++)
            {
                pathElementList.Add(pathElements[i]);
            }
            Path path = new Path(pathElementList, totalPathCost[0]);
            openList.Dispose();
            nodeData.Dispose();
            totalPathCost.Dispose();
            pathElements.Dispose();
            m_valid = false;
            return path;

        }
    }
}