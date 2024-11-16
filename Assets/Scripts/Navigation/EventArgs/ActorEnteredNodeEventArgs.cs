
using System;

namespace Navigation
{
    public class ActorEnteredNodeEventArgs : EventArgs
    {
        public int FromIndex { get; private set; }
        public int ToIndex { get; private set; }

        public ActorEnteredNodeEventArgs(int fromIndex, int toIndex)
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }

    }
}