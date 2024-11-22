
using System;

namespace Navigation
{
    public class ActorExitedNodeEventArgs : EventArgs
    {
        public int FromIndex { get; private set; }
        public int ToIndex { get; private set; }

        public ActorExitedNodeEventArgs(int fromIndex, int toIndex)
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }
    }
}