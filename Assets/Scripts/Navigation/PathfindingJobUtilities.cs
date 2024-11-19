using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

namespace Navigation
{
    public static class PathfindingJobUtilities
    {
        public static void ReversePath(NativeArray<PathElement> _path)
        {
            int length = _path.Length;
            for (int i = 0; i < length / 2; i++)
            {
                PathElement temp = _path[i];
                _path[i] = _path[length - i - 1];
                _path[length - i - 1] = temp;
            }
        }

    }


}